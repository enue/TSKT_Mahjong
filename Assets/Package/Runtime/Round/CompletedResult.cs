#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs.Rounds
{
    public readonly struct CompletedResult
    {
        public readonly Player winner;
        public readonly (int dealer, int player)? tsumoPenalty;
        public readonly int? dealerTsumoPenalty;
        public readonly int? ロン払い;
        public readonly Dictionary<Player, int> scoreDiffs;
        public readonly (int han, int fu)? displayScore;
        public readonly ScoreType? scoreType;
        public readonly Dictionary<役, int> display役s;
        public readonly int dora;
        public readonly int uraDora;
        public readonly int redTile;

        public CompletedResult(CompletedHand source, Player player)
        {
            winner = player;
            scoreType = source.基本点(player.局.game.rule.handCap).type;
            dora = source.Dora;
            uraDora = source.UraDora;
            redTile = source.赤牌;
            if (source.役満.Count > 0)
            {
                display役s = source.役満.ToDictionary(_ => _.Key, _ => 0);
                displayScore = null;
            }
            else
            {
                display役s = source.役;
                if (source.翻 >= 5)
                {
                    displayScore = null;
                }
                else
                {
                    displayScore = (source.翻, source.Fu);
                }
            }

            var asDealer = player.Is親;
            scoreDiffs = new Dictionary<Player, int>();

            if (source.自摸)
            {
                ロン払い = null;
                if (asDealer)
                {
                    dealerTsumoPenalty = 親自摸Penalty(source.基本点(player.局.game.rule.handCap).score, player.局.game.本場);
                    tsumoPenalty = null;

                    foreach (var it in player.局.players)
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
                    tsumoPenalty = 子自摸Penalty(source.基本点(player.局.game.rule.handCap).score, player.局.game.本場);
                    dealerTsumoPenalty = null;

                    foreach (var it in player.局.players)
                    {
                        if (it != player)
                        {
                            if (it.Is親)
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
                ロン払い = RonPenalty(source.基本点(player.局.game.rule.handCap).score, asDealer, player.局.game.本場);
                dealerTsumoPenalty = null;
                tsumoPenalty = null;

                scoreDiffs[source.ronTarget!] = -ロン払い.Value;
                scoreDiffs[player] = ロン払い.Value;
            }
        }

        static int 親自摸Penalty(int basicScore, int 本場)
        {
            return Ceil(basicScore * 2) + 本場 * 100;
        }

        static (int dealer, int player) 子自摸Penalty(int basicScore, int 本場)
        {
            return (
                Ceil(basicScore * 2) + 本場 * 100,
                Ceil(basicScore) + 本場 * 100);
        }

        static int RonPenalty(int basicScore, bool asDealer, int 本場)
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

        static int Ceil(int score)
        {
            var fraction = score % 100;
            if (fraction == 0)
            {
                return score;
            }
            return score + 100 - fraction;
        }

    }
}
