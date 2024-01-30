#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs
{
    public class 局
    {
        public readonly Game game;
        public readonly 壁牌 壁牌;
        /// <summary>
        /// 王牌
        /// </summary>
        public readonly 王牌 王牌 = new();
        public readonly Player[] players = new Player[4];
        /// <summary>
        /// 場風
        /// </summary>
        public readonly TileType 場風;
        /// <summary>
        /// 親
        /// </summary>
        public readonly PlayerIndex 親Index;
        public Player 親 => players[(int)親Index];
        public int CountKan => players.Sum(_ => _.手牌.副露.Count(x => x.槓子));
        public readonly List<Tile> totalDiscardedTiles = new();

        public 局(Game game, TileType 場風, PlayerIndex 親,
            params TileType[]?[]? initialPlayerTilesByCheat)
        {
            this.game = game;
            this.場風 = 場風;
            this.親Index = 親;
            壁牌 = new 壁牌(game.rule.redTile);

            var winds = new TileType[] { TileType.東, TileType.南, TileType.西, TileType.北 };

            for (int i = 0; i < players.Length; ++i)
            {
                var wind = winds[(i - (int)親 + winds.Length) % winds.Length];
                var player = new Player(this, (PlayerIndex)i, wind);
                players[i] = player;
            }

            if (initialPlayerTilesByCheat != null)
            {
                for (int i = 0; i < initialPlayerTilesByCheat.Length; ++i)
                {
                    if (initialPlayerTilesByCheat[i] != null)
                    {
                        foreach (var it in initialPlayerTilesByCheat[i]!)
                        {
                            var t = 壁牌.tiles.FirstOrDefault(_ => _.type == it);
                            if (t != null)
                            {
                                壁牌.tiles.Remove(t);
                                players[i].手牌.tiles.Add(t);
                            }
                        }
                    }
                }
            }

            foreach (var player in players)
            {
                while (player.手牌.tiles.Count < 13)
                {
                    var t = 壁牌.tiles[0];
                    壁牌.tiles.RemoveAt(0);
                    player.手牌.tiles.Add(t);
                }
                player.手牌.Sort();
            }

            for (int i = 0; i < 王牌.Count; ++i)
            {
                var t = 壁牌.tiles[0];
                壁牌.tiles.RemoveAt(0);
                王牌.tiles.Add(t);
            }
            王牌.OpenDora();
        }

        局(in Serializables.局 source)
        {
            壁牌 = source.壁牌.Deserialize();
            王牌 = source.王牌.Deserialize(壁牌);
            親Index = source.親;
            game = source.game.Deserialize();
            players = source.players.Select(_ => _.Deserialize(this)).ToArray();
            場風 = source.場風;
            totalDiscardedTiles = source.totalDiscardedTiles.Select(_ => 壁牌.allTiles[_]).ToList();
        }
        static public 局 FromSerializable(in Serializables.局 source)
        {
            return new 局(source);
        }

        public Serializables.局 ToSerializable()
        {
            return new Serializables.局(this);
        }

        public AfterDraw Start()
        {
            if (親.手牌.tiles.Count == 13)
            {
                return 親.Draw();
            }
            else if (親.手牌.tiles.Count == 14)
            {
                // イカサマ機能によって配牌14枚が固定している場合があるので、手牌14枚を許容する。
                親.OnTurnStart();
                return new AfterDraw(親, null, 嶺上: false, openDoraAfterDiscard: false);
            }
            else
            {
                throw new System.Exception("wrong hand tile count : " + 親.手牌.tiles.Count);
            }
        }

        Tile DrawFrom王牌(Player player)
        {
            var t = 王牌.Draw();
            player.手牌.tiles.Add(t);

            if (壁牌.tiles.Count > 0)
            {
                var wall = 壁牌.tiles[0];
                壁牌.tiles.RemoveAt(0);
                王牌.tiles.Add(wall);
            }
            return t;
        }

        public AfterDraw Execute暗槓(Player player, TileType tileType)
        {
            // 国士無双の槍槓見逃しでフリテンになるが、どのみち上がれないので無視する
            foreach (var it in players)
            {
                it.一発 = false;
            }

            player.手牌.Build暗槓(tileType);

            var t = DrawFrom王牌(player);
            王牌.OpenDora();

            foreach (var it in players)
            {
                if (it == player)
                {
                    continue;
                }
                it.TryAttachFuritenByOtherPlayers(new Tile(0, tileType, false));
            }

            player.OnTurnStart();
            return new AfterDraw(player, t, 嶺上: true, openDoraAfterDiscard: false);
        }

        public AfterDraw Execute加槓(Player player, Tile tile)
        {
            foreach (var it in players)
            {
                if (it == player)
                {
                    continue;
                }
                it.TryAttachFuritenByOtherPlayers(tile);
            }
            foreach (var it in players)
            {
                it.一発 = false;
            }

            var meldIndex = player.手牌.副露.FindIndex(_ => _.tileFroms.All(x => x.tile.type == tile.type));
            var tiles = player.手牌.副露[meldIndex].tileFroms
                .Append((tile, player.index))
                .ToArray();
            player.手牌.副露[meldIndex] = new 副露(tiles);

            player.手牌.tiles.Remove(tile);

            var drawTile = DrawFrom王牌(player);

            if (game.rule.明槓槓ドラ == Rules.明槓槓ドラ.即ノリ)
            {
                王牌.OpenDora();
            }
            foreach (var it in players)
            {
                if (it == player)
                {
                    continue;
                }
                it.TryAttachFuritenByOtherPlayers(tile);
            }

            player.OnTurnStart();
            return new AfterDraw(player, drawTile, 嶺上: true,
                openDoraAfterDiscard: game.rule.明槓槓ドラ == Rules.明槓槓ドラ.打牌後);
        }

        public AfterDraw Execute大明槓(Player player, Player discardPlayer)
        {
            foreach (var it in players)
            {
                it.一発 = false;
            }
            var discardPile = discardPlayer.河;
            var discardTile = discardPile[^1];
            discardPile.RemoveAt(discardPile.Count - 1);

            var meldTiles = new List<(Tile, PlayerIndex playerIndex)>
            {
                (discardTile, discardPlayer.index)
            };
            for (int i = 0; i < 3; ++i)
            {
                var tile = player.手牌.tiles.First(_ => _.type == discardTile.type);
                player.手牌.tiles.Remove(tile);
                meldTiles.Add((tile, player.index));
            }
            var meld = new 副露(meldTiles.ToArray());
            player.手牌.副露.Add(meld);

            var drawTile = DrawFrom王牌(player);

            if (game.rule.明槓槓ドラ == Rules.明槓槓ドラ.即ノリ)
            {
                王牌.OpenDora();
            }

            player.OnTurnStart();
            return new AfterDraw(player, drawTile, 嶺上: true,
                openDoraAfterDiscard: game.rule.明槓槓ドラ == Rules.明槓槓ドラ.打牌後);
        }

        public int HiddenTileCountFrom(Player observer, TileType target)
        {
            var result = 4;
            result -= observer.手牌.tiles.Count(_ => _.type == target);
            foreach (var player in players)
            {
                result -= player.手牌.副露
                    .SelectMany(_ => _.tileFroms)
                    .Count(_ => _.tile.type == target);
                result -= player.河.Count(_ => _.type == target);
            }
            return result;
        }

        public Dictionary<TileType, int> HiddenTileCountFrom(Player observer)
        {
            var result = 壁牌.allTiles
                .Select(_ => _.type)
                .Distinct()
                .ToDictionary(_ => _, _ => 4);

            // 自分の手牌
            foreach (var it in observer.手牌.tiles)
            {
                --result[it.type];
            }
            foreach (var player in players)
            {
                // 副露
                foreach (var meld in player.手牌.副露)
                {
                    foreach (var (tile, _) in meld.tileFroms)
                    {
                        --result[tile.type];
                    }
                }
                // 河
                foreach (var tile in player.河)
                {
                    --result[tile.type];
                }
            }
            // ドラ表示
            foreach (var it in 王牌.ドラ表示牌)
            {
                --result[it.type];
            }

            return result;
        }


    }
}
