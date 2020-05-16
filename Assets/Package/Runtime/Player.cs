﻿using System.Collections;
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
        public bool フリテン;

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

        public AfterDraw Draw()
        {
            if (!Riichi)
            {
                フリテン = false;
            }
            var t = round.wallTile.tiles[0];
            round.wallTile.tiles.RemoveAt(0);
            hand.tiles.Add(t);
            return new AfterDraw(this, t, 嶺上: false, openDoraAfterDiscard: false);
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

        public bool CanChi(TileType discarded, out List<(Tile left, Tile right)> combinations)
        {
            if (Riichi)
            {
                combinations = null;
                return false;
            }
            if (!discarded.IsSuited())
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
                if (it.type.Suit() != discarded.Suit())
                {
                    continue;
                }
                var diff = it.type.Number() - discarded.Number();
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
                combinations.Add((minus2, minus1));
            }
            if (minus1 != null && plus1 != null)
            {
                combinations.Add((minus1, plus1));
            }
            if (plus1 != null && plus2 != null)
            {
                combinations.Add((plus1, plus2));
            }

            return combinations.Count > 0;
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
    }

}
