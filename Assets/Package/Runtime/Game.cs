using System.Collections.Generic;
using System.Linq;
#nullable enable

namespace TSKT.Mahjongs
{
    public class Game
    {
        readonly TileType[] winds = new[] { TileType.東, TileType.南, TileType.西, TileType.北 };
        public int RoundWindCount { get; private set; } = 0;
        public TileType 場風 => winds[RoundWindCount % winds.Length];
        public int DisplayRoundCount { get; private set; } = 1;
        public int リーチ棒スコア;
        public int 本場 { get; private set; }
        public int 連荘 { get; private set; }
        public readonly PlayerIndex 起家;
        public readonly Seat[] seats;
        public readonly RuleSetting rule;
        public readonly List<ScriptableRules.ICompletedResultModifier> completedHandModifiers = new();

        public PlayerIndex 親 => (PlayerIndex)(((int)起家 + DisplayRoundCount - 1) % 4);

        static public AfterDraw Create(PlayerIndex firstDealer, RuleSetting rule)
        {
            var game = new Game(firstDealer, rule);
            return game.StartRound();
        }

        Game(PlayerIndex 起家, RuleSetting rule)
        {
            this.rule = rule;
            this.起家 = 起家;
            seats = new Seat[4];
            for (int i = 0; i < seats.Length; ++i)
            {
                seats[i] = new Seat(rule.initialScore);
            }
        }

        static public Game FromSerializable(in Serializables.Game source)
        {
            var result = new Game(source.firstDealer, source.rule)
            {
                DisplayRoundCount = source.displayRoundCount,
                リーチ棒スコア = source.riichiScore,
                RoundWindCount = source.roundWindCount
            };
            for (int i = 0; i < source.scores.Length; ++i)
            {
                result.seats[i].score = source.scores[i];
            }
            result.本場 = source.本場;
            result.連荘 = source.連荘;

            return result;
        }

        public Serializables.Game ToSerializable()
        {
            return new Serializables.Game(this);
        }

        public AfterDraw StartRound(params TileType[]?[]? initialPlayerTilesByCheat)
        {
            var round = new 局(this, 場風, 親, initialPlayerTilesByCheat);
            return round.Start();
        }

        public AfterDraw? AdvanceRoundBy親上がり(out GameResult? gameResult)
        {
            ++連荘;
            ++本場;

            if (ShouldFinish)
            {
                gameResult = new GameResult(this);
                return null;
            }

            if (rule.end.アガリ止め == Rules.アガリ止め.あり)
            {
                if (Isオーラス)
                {
                    gameResult = new GameResult(this);
                    if (gameResult.playerRanks[親].rank == 1)
                    {
                        return null;
                    }
                }
            }

            gameResult = null;
            return StartRound();
        }
        public AfterDraw? AdvanceRoundBy子上がり(out GameResult? gameResult)
        {
            連荘 = 0;
            本場 = 0;
            if (DisplayRoundCount == 4)
            {
                ++RoundWindCount;
                DisplayRoundCount = 1;
            }
            else
            {
                ++DisplayRoundCount;
            }

            if (ShouldFinish)
            {
                gameResult = new GameResult(this);
                return null;
            }
            gameResult = null;
            return StartRound();
        }

        public AfterDraw? AdvanceRoundByノーテン流局(out GameResult? gameResult)
        {
            return AdvanceRoundBy子上がり(out gameResult);
        }


        public AfterDraw? AdvanceRoundBy途中流局(out GameResult? gameResult)
        {
            return AdvanceRoundByテンパイ流局(out gameResult);
        }

        public AfterDraw? AdvanceRoundByテンパイ流局(out GameResult? gameResult)
        {
            ++本場;

            if (ShouldFinish)
            {
                gameResult = new GameResult(this);
                return null;
            }
            gameResult = null;
            return StartRound();
        }


        bool ShouldFinish
        {
            get
            {
                if (rule.end.endWhenScoreUnderZero)
                {
                    if (seats.Select(_ => _.score).Min() < 0)
                    {
                        return true;
                    }
                }

                if (!IsExtraRound)
                {
                    return false;
                }

                // 延長判定
                // サドンデスでない場合、場風の切り替え時のみ判定
                if (rule.end.suddenDeathInExtraRound
                    || (連荘 == 0 && 本場 == 0 && DisplayRoundCount == 1))
                {
                    var topScore = seats.Max(_ => _.score);
                    if (topScore >= rule.end.extraRoundScoreThreshold)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool Isオーラス
        {
            get
            {
                switch (rule.end.lengthType)
                {
                    case Rules.LengthType.東風戦: return (RoundWindCount == 0) && (DisplayRoundCount == 4);
                    case Rules.LengthType.半荘戦: return (RoundWindCount == 1) && (DisplayRoundCount == 4);
                    case Rules.LengthType.一荘戦: return (RoundWindCount == 3) && (DisplayRoundCount == 4);
                    default:
                        throw new System.ArgumentException(rule.end.lengthType.ToString());
                }
            }
        }

        bool IsExtraRound
        {
            get
            {
                switch(rule.end.lengthType)
                {
                    case Rules.LengthType.東風戦: return RoundWindCount > 0;
                    case Rules.LengthType.半荘戦: return RoundWindCount > 1;
                    case Rules.LengthType.一荘戦: return RoundWindCount > 3;
                    default:
                        throw new System.ArgumentException(rule.end.lengthType.ToString());
                }
            }
        }
    }
}

