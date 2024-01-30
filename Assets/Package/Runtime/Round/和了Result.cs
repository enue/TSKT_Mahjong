#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace TSKT.Mahjongs.Rounds
{
    public readonly struct 和了Result
    {
        public readonly Player winner;
        public readonly (int 親, int 子)? ツモ払い;
        public readonly int? 親ツモ払い;
        public readonly int? ロン払い;
        public readonly Dictionary<Player, int> scoreDiffs;
        public readonly (int 翻, int 符)? displayScore;
        public readonly ScoreType? scoreType;
        public readonly Dictionary<役, int> display役s;
        public readonly int ドラ;
        public readonly int 裏ドラ;
        public readonly int 赤牌;

        public 和了Result(和了 source, Player player)
        {
            winner = player;
            scoreType = source.基本点(player.局.game.rule.役満複合).type;
            ドラ = source.ドラ;
            裏ドラ = source.裏ドラ;
            赤牌 = source.赤牌;
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
                    displayScore = (source.翻, source.符);
                }
            }

            var asDealer = player.Is親;
            scoreDiffs = new Dictionary<Player, int>();

            if (source.自摸)
            {
                ロン払い = null;
                if (asDealer)
                {
                    親ツモ払い = 親自摸Penalty(source.基本点(player.局.game.rule.役満複合).score, player.局.game.本場);
                    ツモ払い = null;

                    foreach (var it in player.局.players)
                    {
                        if (it != player)
                        {
                            scoreDiffs[it] = -親ツモ払い.Value;
                        }
                    }
                    scoreDiffs[player] = 親ツモ払い.Value * 3;
                }
                else
                {
                    ツモ払い = 子自摸Penalty(source.基本点(player.局.game.rule.役満複合).score, player.局.game.本場);
                    親ツモ払い = null;

                    foreach (var it in player.局.players)
                    {
                        if (it != player)
                        {
                            if (it.Is親)
                            {
                                scoreDiffs[it] = -ツモ払い.Value.親;
                            }
                            else
                            {
                                scoreDiffs[it] = -ツモ払い.Value.子;
                            }
                        }
                    }
                    scoreDiffs[player] = ツモ払い.Value.親 + ツモ払い.Value.子 * 2;
                }
            }
            else
            {
                ロン払い = ロンPenalty(source.基本点(player.局.game.rule.役満複合).score, asDealer, player.局.game.本場);
                親ツモ払い = null;
                ツモ払い = null;

                scoreDiffs[source.ronTarget!] = -ロン払い.Value;
                scoreDiffs[player] = ロン払い.Value;
            }
        }

        static int 親自摸Penalty(int basicScore, int 本場)
        {
            return Ceil(basicScore * 2) + 本場 * 100;
        }

        static (int 親, int 子) 子自摸Penalty(int basicScore, int 本場)
        {
            return (
                Ceil(basicScore * 2) + 本場 * 100,
                Ceil(basicScore) + 本場 * 100);
        }

        static int ロンPenalty(int basicScore, bool asDealer, int 本場)
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
