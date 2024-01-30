using System.Collections;
using System.Collections.Generic;
using System.Linq;
#nullable enable

namespace TSKT.Mahjongs.Rounds
{
    public class Player
    {
        readonly public 局 局;
        readonly public PlayerIndex index;
        readonly public 手牌 手牌;
        readonly public List<Tile> 河 = new();
        readonly public List<Tile> 捨て牌 = new();
        readonly public TileType 自風;
        public int? RiichiIndexInDiscardPile { get; private set; }
        public int? RiichiIndexInTotalDiscardTiles { get; private set; }

        public bool ダブルリーチ { get; private set; }
        public bool オープンリーチ { get; private set; }
        public bool 一発;
        public bool フリテンByOtherPlayers { get; private set; }
        public bool フリテン => フリテンByOtherPlayers || フリテンByMyself;

        public bool リーチ
        {
            get => RiichiIndexInDiscardPile.HasValue;
            private set
            {
                if (value)
                {
                    if (!RiichiIndexInDiscardPile.HasValue)
                    {
                        RiichiIndexInDiscardPile = 河.Count;
                        RiichiIndexInTotalDiscardTiles = 局.totalDiscardedTiles.Count;
                        Score -= 1000;
                        局.game.リーチ棒スコア += 1000;
                    }
                }
                else
                {
                    RiichiIndexInDiscardPile = null;
                }
            }
        }

        public bool Is親 => 局.親Index == index;

        public int Score
        {
            get => 局.game.seats[(int)index].score;
            set => 局.game.seats[(int)index].score = value;
        }

        public Player(局 局, PlayerIndex index, TileType 自風)
        {
            this.局 = 局;
            this.index = index;
            this.自風 = 自風;
            手牌 = new 手牌(this);
        }

        Player(in Serializables.Player source, 局 局)
        {
            this.局 = 局;

            捨て牌 = source.捨て牌.Select(_ => 局.壁牌.allTiles[_]).ToList();
            河 = source.河.Select(_ => 局.壁牌.allTiles[_]).ToList();
            ダブルリーチ = source.ダブルリーチ;
            手牌 = source.手牌.Deserialize(this);
            index = source.index;
            オープンリーチ = source.オープンリーチ;
            RiichiIndexInDiscardPile = (source.riichiIndexInDiscardPile >= 0) ? source.riichiIndexInDiscardPile : (int?)null;
            RiichiIndexInTotalDiscardTiles = (source.riichiIndexInTotalDiscardTiles >= 0) ? source.riichiIndexInTotalDiscardTiles : (int?)null;
            自風 = source.自風;
            フリテンByOtherPlayers = source.フリテンByOtherPlayers;
            一発 = source.一発;
        }

        public static Player FromSerializable(in Serializables.Player source, 局 round)
        {
            return new Player(source, round);
        }

        public Serializables.Player ToSerializable()
        {
            return new Serializables.Player(this);
        }

        public AfterDraw Draw()
        {
            var t = 局.壁牌.tiles[0];
            局.壁牌.tiles.RemoveAt(0);
            手牌.tiles.Add(t);

            OnTurnStart();
            return new AfterDraw(this, t, 嶺上: false, openDoraAfterDiscard: false);
        }

        public void OnTurnStart()
        {
            if (!リーチ)
            {
                フリテンByOtherPlayers = false;
            }
        }

        public void Discard(Tile tile, bool リーチ, bool オープンリーチ)
        {
            if (リーチ || オープンリーチ)
            {
                if (局.players.All(_ => _.手牌.副露.Count == 0)
                    && 捨て牌.Count == 0)
                {
                    ダブルリーチ = true;
                }
                this.リーチ = true;
                一発 = true;
                this.オープンリーチ = オープンリーチ;
            }
            else
            {
                一発 = false;
            }

            手牌.tiles.Remove(tile);
            河.Add(tile);
            捨て牌.Add(tile);
            局.totalDiscardedTiles.Add(tile);
        }

        // 赤牌ルールだと、どの牌で鳴くか選択肢があるケースがある
        public bool CanPon(TileType tile, out List<(Tile left, Tile right)>? combinations)
        {
            if (リーチ)
            {
                combinations = null;
                return false;
            }
            var tiles = 手牌.tiles.Where(_ => _.type == tile).ToArray();
            if (tiles.Length < 2)
            {
                combinations = null;
                return false;
            }

            combinations = new List<(Tile left, Tile right)>();
            for (int i = 0; i < tiles.Length - 1; ++i)
            {
                var left = tiles[i];
                for (int j = i + 1; j < tiles.Length; ++j)
                {
                    var right = tiles[j];
                    combinations.Add((left, right));
                }
            }
            return true;
        }

        public bool CanChi(Tile discarded, out List<(Tile left, Tile right)>? combinations)
        {
            if (リーチ)
            {
                combinations = null;
                return false;
            }
            if (!discarded.type.Is数牌())
            {
                combinations = null;
                return false;
            }


            var minus2Tiles = System.Array.Empty<Tile>();
            if (discarded.type.Number() > 2)
            {
                var minus2 = TileTypeUtil.Get(discarded.type.Suit(), discarded.type.Number() - 2);
                minus2Tiles = 手牌.tiles.Where(_ => _.type == minus2).ToArray();
            }

            var minus1Tiles = System.Array.Empty<Tile>();
            if (discarded.type.Number() > 1)
            {
                var minus1 = TileTypeUtil.Get(discarded.type.Suit(), discarded.type.Number() - 1);
                minus1Tiles = 手牌.tiles.Where(_ => _.type == minus1).ToArray();
            }

            var plus1Tiles = System.Array.Empty<Tile>();
            if (discarded.type.Number() < 9)
            {
                var plus1 = TileTypeUtil.Get(discarded.type.Suit(), discarded.type.Number() + 1);
                plus1Tiles = 手牌.tiles.Where(_ => _.type == plus1).ToArray();
            }

            var plus2Tiles = System.Array.Empty<Tile>();
            if (discarded.type.Number() < 8)
            {
                var plus2 = TileTypeUtil.Get(discarded.type.Suit(), discarded.type.Number() + 2);
                plus2Tiles = 手牌.tiles.Where(_ => _.type == plus2).ToArray();
            }

            var tilePairs = new[]
            {
                (leftTiles: minus1Tiles, rightTiles: minus2Tiles),
                (leftTiles: minus1Tiles, rightTiles: plus1Tiles),
                (leftTiles: plus1Tiles, rightTiles: plus2Tiles),
            };

            combinations = new List<(Tile left, Tile right)>();
            foreach (var (leftTiles, rightTiles) in tilePairs)
            {
                if (leftTiles.Length > 0 && rightTiles.Length > 0)
                {
                    if (CanDiscardAfterChi(discarded, leftTiles[0], rightTiles[0]))
                    {
                        foreach(var left in leftTiles)
                        {
                            foreach(var right in rightTiles)
                            {
                                combinations.Add((left, right));
                            }
                        }
                    }
                }
            }
            return combinations.Count > 0;
        }

        bool CanDiscardAfterChi(Tile tileFromOtherPlayer, Tile left, Tile right)
        {
            if (局.game.rule.喰い替え == Rules.喰い替え.あり)
            {
                return true;
            }

            var meld = new 副露(
                (left, index),
                (right, index),
                (tileFromOtherPlayer, (PlayerIndex)((int)(index + 3) % 4)));
            return 手牌.tiles
                .Where(_ => _ != left && _ != right)
                .Any(_ => !meld.Is喰い替え(_));
        }

        public bool Can暗槓(TileType tile)
        {
            if (局.CountKan == 4)
            {
                return false;
            }

            if (手牌.tiles.Count(_ => _.type == tile) < 4)
            {
                return false;
            }

            if (リーチ)
            {
                // 待ち牌が変わる暗槓はできない
                TileType[] 和了牌BeforeDraw;
                TileType[] 和了牌After暗槓;
                {
                    var clone = 手牌.Clone();
                    var drewTileIndex = clone.tiles.FindIndex(_ => _.type == tile);
                    clone.tiles.RemoveAt(drewTileIndex);
                    和了牌BeforeDraw = clone.Get和了牌();
                }
                {
                    var clone = 手牌.Clone();
                    clone.Build暗槓(tile);
                    和了牌After暗槓 = clone.Get和了牌();
                }
                if (!和了牌BeforeDraw.SequenceEqual(和了牌After暗槓))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Can大明槓(TileType tile)
        {
            if (リーチ)
            {
                return false;
            }
            if (局.CountKan == 4)
            {
                return false;
            }
            return 手牌.tiles.Count(_ => _.type == tile) >= 3;
        }
        public bool Can加槓(TileType tile)
        {
            if (リーチ)
            {
                return false;
            }
            if (局.CountKan == 4)
            {
                return false;
            }
            return 手牌.副露.Any(_ => _.tileFroms.All(x => x.tile.type == tile));
        }

        public RelativePlayer GetRelativePlayer(Player target)
        {
            return RelativePlayerUtil.GetByPlayerIndex(index, target.index);
        }

        public void TryAttachFuritenByOtherPlayers(Tile tile)
        {
            if (フリテンByOtherPlayers)
            {
                return;
            }

            var cloneHand = 手牌.Clone();
            cloneHand.tiles.Add(tile);
            if (cloneHand.向聴数IsLessThanOrEqual(-1))
            {
                フリテンByOtherPlayers = true;
            }
        }

        bool フリテンByMyself
        {
            get
            {
                if (手牌.tiles.Count % 3 != 1)
                {
                    return false;
                }

                var discardedTiles = this.捨て牌
                    .Select(_ => _.type)
                    .Distinct();
                foreach(var it in discardedTiles)
                {
                    var cloneHand = 手牌.Clone();
                    cloneHand.tiles.Add(new Tile(0, it, false));
                    if (cloneHand.向聴数IsLessThanOrEqual(-1))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public Dictionary<TileType, int> HiddenTileCounts => 局.HiddenTileCountFrom(this);
        public int HiddenTileCount(TileType t) => 局.HiddenTileCountFrom(this, t);
    }
}
