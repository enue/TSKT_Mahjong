using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#nullable enable

namespace TSKT.Mahjongs
{
    public class Round
    {
        public readonly Game game;
        public readonly WallTile wallTile;
        public readonly DeadWallTile deadWallTile = new DeadWallTile();
        public readonly Player[] players = new Player[4];
        public readonly TileType roundWind;
        public readonly PlayerIndex dealer;
        public Player Dealer => players[(int)dealer];
        public int CountKan => players.Sum(_ => _.hand.melds.Count(x => x.槓子));
        public readonly List<Tile> totalDiscardedTiles = new List<Tile>();

        public Round(Game game, TileType prevailingWind, PlayerIndex dealer,
            params TileType[][] initialPlayerTilesByCheat)
        {
            this.game = game;
            roundWind = prevailingWind;
            this.dealer = dealer;
            wallTile = new WallTile(game.rule.redTile);

            var winds = new TileType[] { TileType.東, TileType.南, TileType.西, TileType.北 };

            for (int i = 0; i < players.Length; ++i)
            {
                var wind = winds[(i - (int)dealer + winds.Length) % winds.Length];
                var player = new Player(game.scoreOwners[i], this, (PlayerIndex)i, wind);
                players[i] = player;
            }

            if (initialPlayerTilesByCheat != null)
            {
                for (int i = 0; i < initialPlayerTilesByCheat.Length; ++i)
                {
                    if (initialPlayerTilesByCheat[i] != null)
                    {
                        foreach (var it in initialPlayerTilesByCheat[i])
                        {
                            var t = wallTile.tiles.FirstOrDefault(_ => _.type == it);
                            if (t != null)
                            {
                                wallTile.tiles.Remove(t);
                                players[i].hand.tiles.Add(t);
                            }
                        }
                    }
                }
            }

            foreach (var player in players)
            {
                while (player.hand.tiles.Count < 13)
                {
                    var t = wallTile.tiles[0];
                    wallTile.tiles.RemoveAt(0);
                    player.hand.tiles.Add(t);
                }
                player.hand.Sort();
            }

            for (int i = 0; i < DeadWallTile.Count; ++i)
            {
                var t = wallTile.tiles[0];
                wallTile.tiles.RemoveAt(0);
                deadWallTile.tiles.Add(t);
            }
            deadWallTile.OpenDora();
        }

        Round(in Serializables.Round source)
        {
            wallTile = source.wallTile.Deserialzie();
            deadWallTile = source.deadWallTile.Deserialzie(wallTile);
            dealer = source.dealer;
            game = source.game.Deserialzie();
            players = source.players.Select(_ => _.Deserialzie(this)).ToArray();
            roundWind = source.roundWind;
            totalDiscardedTiles = source.totalDiscardedTiles.Select(_ => wallTile.allTiles[_]).ToList();
        }
        static public Round FromSerializable(in Serializables.Round source)
        {
            return new Round(source);
        }

        public Serializables.Round ToSerializable()
        {
            return new Serializables.Round(this);
        }

        public AfterDraw Start()
        {
            if (Dealer.hand.tiles.Count == 13)
            {
                return Dealer.Draw();
            }
            else if (Dealer.hand.tiles.Count == 14)
            {
                // イカサマ機能によって配牌14枚が固定している場合があるので、手牌14枚を許容する。
                Dealer.OnTurnStart();
                return new AfterDraw(Dealer, null, 嶺上: false, openDoraAfterDiscard: false);
            }
            else
            {
                throw new System.Exception("wrong hand tile count : " + Dealer.hand.tiles.Count);
            }
        }

        Tile DrawFromDeadWallTile(Player player)
        {
            var t = deadWallTile.Draw();
            player.hand.tiles.Add(t);

            if (wallTile.tiles.Count > 0)
            {
                var wall = wallTile.tiles[0];
                wallTile.tiles.RemoveAt(0);
                deadWallTile.tiles.Add(wall);
            }
            return t;
        }

        public AfterDraw ExecuteClosedQuad(Player player, TileType tileType)
        {
            player.hand.BuildClosedQuad(tileType);

            var t = DrawFromDeadWallTile(player);
            deadWallTile.OpenDora();

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

        public AfterDraw ExecuteAddedOpenQuad(Player player, Tile tile)
        {
            var meldIndex = player.hand.melds.FindIndex(_ => _.tileFroms.All(x => x.tile.type == tile.type));
            var tiles = player.hand.melds[meldIndex].tileFroms
                .Append((tile, player.index))
                .ToArray();
            player.hand.melds[meldIndex] = new Meld(tiles);

            player.hand.tiles.Remove(tile);

            var drawTile = DrawFromDeadWallTile(player);

            if (game.rule.明槓槓ドラ == Rules.明槓槓ドラ.即ノリ)
            {
                deadWallTile.OpenDora();
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

        public AfterDraw ExecuteOpenQuad(Player player, Player discardPlayer)
        {
            var discardPile = discardPlayer.discardPile;
            var discardTile = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            var meldTiles = new List<(Tile, PlayerIndex playerIndex)>();
            meldTiles.Add((discardTile, discardPlayer.index));
            for (int i = 0; i < 3; ++i)
            {
                var tile = player.hand.tiles.First(_ => _.type == discardTile.type);
                player.hand.tiles.Remove(tile);
                meldTiles.Add((tile, player.index));
            }
            var meld = new Meld(meldTiles.ToArray());
            player.hand.melds.Add(meld);

            var drawTile = DrawFromDeadWallTile(player);

            if (game.rule.明槓槓ドラ == Rules.明槓槓ドラ.即ノリ)
            {
                deadWallTile.OpenDora();
            }

            player.OnTurnStart();
            return new AfterDraw(player, drawTile, 嶺上: true,
                openDoraAfterDiscard: game.rule.明槓槓ドラ == Rules.明槓槓ドラ.打牌後);
        }

        public int HiddenTileCountFrom(Player observer, TileType target)
        {
            var result = 4;
            result -= observer.hand.tiles.Count(_ => _.type == target);
            foreach (var player in players)
            {
                result -= player.hand.melds
                    .SelectMany(_ => _.tileFroms)
                    .Count(_ => _.tile.type == target);
                result -= player.discardPile.Count(_ => _.type == target);
            }
            return result;
        }

        public Dictionary<TileType, int> HiddenTileCountFrom(Player observer)
        {
            var result = wallTile.allTiles
                .Select(_ => _.type)
                .Distinct()
                .ToDictionary(_ => _, _ => 4);

            // 自分の手牌
            foreach (var it in observer.hand.tiles)
            {
                --result[it.type];
            }
            foreach (var player in players)
            {
                // 副露
                foreach (var meld in player.hand.melds)
                {
                    foreach (var (tile, _) in meld.tileFroms)
                    {
                        --result[tile.type];
                    }
                }
                // 河
                foreach (var tile in player.discardPile)
                {
                    --result[tile.type];
                }
            }
            // ドラ表示
            foreach (var it in deadWallTile.doraIndicatorTiles)
            {
                --result[it.type];
            }

            return result;
        }


    }
}
