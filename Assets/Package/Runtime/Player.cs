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
        readonly public int index;
        readonly public Hand hand;
        readonly public List<Tile> discardPile = new List<Tile>();
        readonly public List<Tile> discardedTiles = new List<Tile>();
        readonly public TileType wind;
        public int? RiichiIndexInDiscardPile { get; private set; }
        public int? RiichiIndexInTotalDiscardTiles { get; private set; }

        public bool DoubleRiichi { get; private set; }
        public bool OpenRiichi { get; private set; }
        public bool 一発;
        public bool 他家によるフリテン { get; private set; }
        public bool フリテン => 他家によるフリテン || 自分によるフリテン;

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

        public Player(ScoreOwner scoreOwner, Round round, int index, TileType wind)
        {
            this.scoreOwner = scoreOwner;
            this.round = round;
            this.index = index;
            this.wind = wind;
            hand = new Hand(this);
        }

        Player(Serializables.Player source, Round round)
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
            他家によるフリテン = source.他家によるフリテン;
            一発 = source.一発;

            scoreOwner = round.game.scoreOwners[source.index];
        }

        public static Player FromSerializable(Serializables.Player source, Round round)
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
                他家によるフリテン = false;
            }
        }

        public void Discard(Tile tile, bool riichi, bool openRiichi)
        {
            if (riichi)
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

            foreach (var player in round.players)
            {
                if (player == this)
                {
                    continue;
                }
                player.Judge他家によるフリテン(tile);
            }
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

            Tile minus2 = null;
            Tile minus1 = null;
            Tile plus1 = null;
            Tile plus2 = null;

            foreach(var it in hand.tiles)
            {
                if (!it.type.IsSuited())
                {
                    continue;
                }
                if (it.type.Suit() != discarded.type.Suit())
                {
                    continue;
                }
                var diff = it.type.Number() - discarded.type.Number();
                if (diff == -2)
                {
                    minus2 = it;
                }
                if (diff == -1)
                {
                    minus1 = it;
                }
                if (diff == 1)
                {
                    plus1 = it;
                }
                if (diff == 2)
                {
                    plus2 = it;
                }
            }

            combinations = new List<(Tile left, Tile right)>();
            if (minus2 != null && minus1 != null)
            {
                if (CanDiscardAfterChi(discarded, minus1, minus2))
                {
                    combinations.Add((minus2, minus1));
                }
            }
            if (minus1 != null && plus1 != null)
            {
                if (CanDiscardAfterChi(discarded, minus1, plus1))
                {
                    combinations.Add((minus1, plus1));
                }
            }
            if (plus1 != null && plus2 != null)
            {
                if (CanDiscardAfterChi(discarded, plus1, plus2))
                {
                    combinations.Add((plus1, plus2));
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

            var meld = new Meld();
            meld.tileFroms.Add((left, index));
            meld.tileFroms.Add((right, index));
            meld.tileFroms.Add((tileFromOtherPlayer, -1));
            return hand.tiles
                .Where(_ => _ != left && _ != right)
                .Any(_ => !meld.Is喰い替え(_, index));
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
                var winningTiles = hand.GetWinningTiles();
                var clone = hand.Clone();
                clone.BuildClosedQuad(tile);
                var winningTileAfterClosedQuad = clone.GetWinningTiles();
                if (!winningTiles.SequenceEqual(winningTileAfterClosedQuad))
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

        public void Judge他家によるフリテン(Tile tile)
        {
            if (他家によるフリテン)
            {
                return;
            }

            var hand = this.hand.Clone();
            hand.tiles.Add(tile);
            if (hand.Solve().向聴数 == -1)
            {
                他家によるフリテン = true;
            }
        }

        bool 自分によるフリテン
        {
            get
            {
                var discardedTiles = this.discardedTiles
                    .Select(_ => _.type)
                    .Distinct();
                foreach(var it in discardedTiles)
                {
                    var h = hand.Clone();
                    h.tiles.Add(new Tile(0, it, false));
                    if (h.Solve().向聴数 == -1)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
