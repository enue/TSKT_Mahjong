using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public readonly struct CompletedResult
    {
        public readonly (double dealer, double player)? tsumoPenalty;
        public readonly double? dealerTsumoPenalty;
        public readonly double? ronPenalty;
        public readonly Dictionary<Player, double> scoreDiffs;
        public readonly (int han, int fu)? displayScore;
        public readonly ScoreType? scoreType;
        public readonly Dictionary<役, int> display役s;
        public readonly int dora;
        public readonly int uraDora;
        public readonly int redTile;

        public CompletedResult(CompletedHand source, Player player)
        {
            scoreType = source.基本点(player.round.game.rule).type;
            dora = source.Dora;
            uraDora = source.UraDora;
            redTile = source.RedTile;
            if (source.役満.Count > 0)
            {
                display役s = source.役満.ToDictionary(_ => _.Key, _ => 0);
                displayScore = null;
            }
            else
            {
                display役s = source.Yakus;
                if (source.Han >= 5 && player.round.game.rule.青天井 == Rules.青天井.なし)
                {
                    displayScore = null;
                }
                else
                {
                    displayScore = (source.Han, source.Fu);
                }
            }

            var asDealer = player.IsDealer;
            scoreDiffs = new Dictionary<Player, double>();

            if (source.自摸)
            {
                ronPenalty = null;
                if (asDealer)
                {
                    dealerTsumoPenalty = 親自摸Penalty(source.基本点(player.round.game.rule).score, player.round.game.本場);
                    tsumoPenalty = null;

                    foreach (var it in player.round.players)
                    {
                        if (it != player)
                        {
                            scoreDiffs[it] = -dealerTsumoPenalty.Value;
                        }
                    }
                    scoreDiffs[player] = dealerTsumoPenalty.Value * 3;
                }
                else
                {
                    tsumoPenalty = 子自摸Penalty(source.基本点(player.round.game.rule).score, player.round.game.本場);
                    dealerTsumoPenalty = null;

                    foreach (var it in player.round.players)
                    {
                        if (it != player)
                        {
                            if (it.IsDealer)
                            {
                                scoreDiffs[it] = -tsumoPenalty.Value.dealer;
                            }
                            else
                            {
                                scoreDiffs[it] = -tsumoPenalty.Value.player;
                            }
                        }
                    }
                    scoreDiffs[player] = tsumoPenalty.Value.dealer + tsumoPenalty.Value.player * 2;
                }
            }
            else
            {
                ronPenalty = RonPenalty(source.基本点(player.round.game.rule).score, asDealer, player.round.game.本場);
                dealerTsumoPenalty = null;
                tsumoPenalty = null;

                scoreDiffs[source.ronTarget] = -ronPenalty.Value;
                scoreDiffs[player] = ronPenalty.Value;
            }
        }

        static double 親自摸Penalty(double basicScore, int 本場)
        {
            return Ceil(basicScore * 2) + 本場 * 100;
        }

        static (double dealer, double player) 子自摸Penalty(double basicScore, int 本場)
        {
            return (
                Ceil(basicScore * 2) + 本場 * 100,
                Ceil(basicScore) + 本場 * 100);
        }

        static double RonPenalty(double basicScore, bool asDealer, int 本場)
        {
            if (asDealer)
            {
                return Ceil(basicScore * 6) + 本場 * 300;
            }
            else
            {
                return Ceil(basicScore * 4) + 本場 * 300;
            }
        }

        static double Ceil(double score)
        {
            var fraction = score % 100.0;
            if (fraction == 0)
            {
                return score;
            }
            return score + 100.0 - fraction;
        }

    }
}
