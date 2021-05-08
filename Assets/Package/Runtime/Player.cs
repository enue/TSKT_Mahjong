using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class Player
    {
        readonly public ScoreOwner scoreOwner;
        readonly public Round round;
        readonly public PlayerIndex index;
        readonly public Hand hand;
        readonly public List<Tile> discardPile = new List<Tile>();
        readonly public List<Tile> discardedTiles = new List<Tile>();
        readonly public TileType wind;
        public int? RiichiIndexInDiscardPile { get; private set; }
        public int? RiichiIndexInTotalDiscardTiles { get; private set; }

        public bool DoubleRiichi { get; private set; }
        public bool OpenRiichi { get; private set; }
        public bool 一発;
        public bool FuritenByOtherPlayers { get; private set; }
        public bool Furiten => FuritenByOtherPlayers || FuritenByMyself;

        public bool Riichi
        {
            get => RiichiIndexInDiscardPile.HasValue;
            private set
            {
                if (value)
                {
                    if (!RiichiIndexInDiscardPile.HasValue)
                    {
                        RiichiIndexInDiscardPile = discardPile.Count;
                        RiichiIndexInTotalDiscardTiles = round.totalDiscardedTiles.Count;
                        scoreOwner.score -= 1000;
                        scoreOwner.game.riichiScore += 1000;
                    }
                }
                else
                {
                    RiichiIndexInDiscardPile = null;
                }
            }
        }

        public bool IsDealer => round.dealer == index;

        public Player(ScoreOwner scoreOwner, Round round, PlayerIndex index, TileType wind)
        {
            this.scoreOwner = scoreOwner;
            this.round = round;
            this.index = index;
            this.wind = wind;
            hand = new Hand(this);
        }

        Player(in Serializables.Player source, Round round)
        {
            this.round = round;

            discardedTiles = source.discardedTiles.Select(_ => round.wallTile.allTiles[_]).ToList();
            discardPile = source.discardPile.Select(_ => round.wallTile.allTiles[_]).ToList();
            DoubleRiichi = source.doubleRiichi;
            hand = source.hand.Deserialzie(this);
            index = source.index;
            OpenRiichi = source.openRiichi;
            RiichiIndexInDiscardPile = (source.riichiIndexInDiscardPile >= 0) ? source.riichiIndexInDiscardPile : (int?)null;
            RiichiIndexInTotalDiscardTiles = (source.riichiIndexInTotalDiscardTiles >= 0) ? source.riichiIndexInTotalDiscardTiles : (int?)null;
            wind = source.wind;
            FuritenByOtherPlayers = source.furitenByOtherPlayers;
            一発 = source.一発;

            scoreOwner = round.game.scoreOwners[(int)source.index];
        }

        public static Player FromSerializable(in Serializables.Player source, Round round)
        {
            return new Player(source, round);
        }

        public Serializables.Player ToSerializable()
        {
            return new Serializables.Player(this);
        }

        public AfterDraw Draw()
        {
            var t = round.wallTile.tiles[0];
            round.wallTile.tiles.RemoveAt(0);
            hand.tiles.Add(t);

            OnTurnStart();
            return new AfterDraw(this, t, 嶺上: false, openDoraAfterDiscard: false);
        }

        public void OnTurnStart()
        {
            if (!Riichi)
            {
                FuritenByOtherPlayers = false;
            }
        }

        public void Discard(Tile tile, bool riichi, bool openRiichi)
        {
            if (riichi || openRiichi)
            {
                if (round.players.All(_ => _.hand.melds.Count == 0)
                    && discardedTiles.Count == 0)
                {
                    DoubleRiichi = true;
                }
                Riichi = true;
                一発 = true;
                OpenRiichi = openRiichi;
            }
            else
            {
                一発 = false;
            }

            hand.tiles.Remove(tile);
            discardPile.Add(tile);
            discardedTiles.Add(tile);
            round.totalDiscardedTiles.Add(tile);
        }

        // 赤牌ルールだと、どの牌で鳴くか選択肢があるケースがある
        public bool CanPon(TileType tile, out List<(Tile left, Tile right)> combinations)
        {
            if (Riichi)
            {
                combinations = null;
                return false;
            }
            var tiles = hand.tiles.Where(_ => _.type == tile).ToArray();
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

        public bool CanChi(Tile discarded, out List<(Tile left, Tile right)> combinations)
        {
            if (Riichi)
            {
                combinations = null;
                return false;
            }
            if (!discarded.type.IsSuited())
            {
                combinations = null;
                return false;
            }


            var minus2Tiles = System.Array.Empty<Tile>();
            if (discarded.type.Number() > 2)
            {
                var minus2 = TileTypeUtil.Get(discarded.type.Suit(), discarded.type.Number() - 2);
                minus2Tiles = hand.tiles.Where(_ => _.type == minus2).ToArray();
            }

            var minus1Tiles = System.Array.Empty<Tile>();
            if (discarded.type.Number() > 1)
            {
                var minus1 = TileTypeUtil.Get(discarded.type.Suit(), discarded.type.Number() - 1);
                minus1Tiles = hand.tiles.Where(_ => _.type == minus1).ToArray();
            }

            var plus1Tiles = System.Array.Empty<Tile>();
            if (discarded.type.Number() < 9)
            {
                var plus1 = TileTypeUtil.Get(discarded.type.Suit(), discarded.type.Number() + 1);
                plus1Tiles = hand.tiles.Where(_ => _.type == plus1).ToArray();
            }

            var plus2Tiles = System.Array.Empty<Tile>();
            if (discarded.type.Number() < 8)
            {
                var plus2 = TileTypeUtil.Get(discarded.type.Suit(), discarded.type.Number() + 2);
                plus2Tiles = hand.tiles.Where(_ => _.type == plus2).ToArray();
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
            if (round.game.rule.喰い替え == Rules.喰い替え.あり)
            {
                return true;
            }

            var meld = new Meld(
                (left, index),
                (right, index),
                (tileFromOtherPlayer, null));
            return hand.tiles
                .Where(_ => _ != left && _ != right)
                .Any(_ => !meld.Is喰い替え(_));
        }

        public bool CanClosedQuad(TileType tile)
        {
            if (round.CountKan == 4)
            {
                return false;
            }

            if (hand.tiles.Count(_ => _.type == tile) < 4)
            {
                return false;
            }

            if (Riichi)
            {
                // 待ち牌が変わる暗槓はできない
                TileType[] winningTilesBeforeDraw;
                TileType[] winningTilesAfterClosedQuad;
                {
                    var clone = hand.Clone();
                    var drewTileIndex = clone.tiles.FindIndex(_ => _.type == tile);
                    clone.tiles.RemoveAt(drewTileIndex);
                    winningTilesBeforeDraw = clone.GetWinningTiles();
                }
                {
                    var clone = hand.Clone();
                    clone.BuildClosedQuad(tile);
                    winningTilesAfterClosedQuad = clone.GetWinningTiles();
                }
                if (!winningTilesBeforeDraw.SequenceEqual(winningTilesAfterClosedQuad))
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanOpenQuad(TileType tile)
        {
            if (Riichi)
            {
                return false;
            }
            if (round.CountKan == 4)
            {
                return false;
            }
            return hand.tiles.Count(_ => _.type == tile) >= 3;
        }
        public bool CanAddedOpenQuad(TileType tile)
        {
            if (Riichi)
            {
                return false;
            }
            if (round.CountKan == 4)
            {
                return false;
            }
            return hand.melds.Any(_ => _.tileFroms.All(x => x.tile.type == tile));
        }

        public RelativePlayer GetRelativePlayer(Player target)
        {
            return RelativePlayerUtil.GetByPlayerIndex(index, target.index);
        }

        public void TryAttachFuritenByOtherPlayers(Tile tile)
        {
            if (FuritenByOtherPlayers)
            {
                return;
            }

            var cloneHand = hand.Clone();
            cloneHand.tiles.Add(tile);
            if (cloneHand.向聴数IsLessThanOrEqual(-1))
            {
                FuritenByOtherPlayers = true;
            }
        }

        bool FuritenByMyself
        {
            get
            {
                if (hand.tiles.Count % 3 != 1)
                {
                    return false;
                }

                var discardedTiles = this.discardedTiles
                    .Select(_ => _.type)
                    .Distinct();
                foreach(var it in discardedTiles)
                {
                    var cloneHand = hand.Clone();
                    cloneHand.tiles.Add(new Tile(0, it, false));
                    if (cloneHand.向聴数IsLessThanOrEqual(-1))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
