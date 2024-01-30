using System.Collections.Generic;
using System.Linq;
#nullable enable

namespace TSKT.Mahjongs
{
    public class Game
    {
        readonly TileType[] winds = new[] { TileType.東, TileType.南, TileType.西, TileType.北 };
        public int 場風Index { get; private set; } = 0;
        public TileType 場風 => winds[場風Index % winds.Length];
        public int Display局 { get; private set; } = 1;
        public int リーチ棒スコア;
        public int 本場 { get; private set; }
        public int 連荘 { get; private set; }
        public readonly PlayerIndex 起家;
        public readonly Seat[] seats;
        public readonly RuleSetting rule;
        public readonly List<ScriptableRules.ICompletedResultModifier> completedHandModifiers = new();

        public PlayerIndex 親 => (PlayerIndex)(((int)起家 + Display局 - 1) % 4);

        static public AfterDraw Create(PlayerIndex 起家, RuleSetting rule)
        {
            var game = new Game(起家, rule);
            return game.Start局();
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
            var result = new Game(source.起家, source.rule)
            {
                Display局 = source.display局,
                リーチ棒スコア = source.リーチ棒スコア,
                場風Index = source.場風Index
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

        public AfterDraw Start局(params TileType[]?[]? initialPlayerTilesByCheat)
        {
            var round = new 局(this, 場風, 親, initialPlayerTilesByCheat);
            return round.Start();
        }

        public AfterDraw? Advance局By親上がり(out GameResult? gameResult)
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
            return Start局();
        }
        public AfterDraw? Advance局By子上がり(out GameResult? gameResult)
        {
            連荘 = 0;
            本場 = 0;
            if (Display局 == 4)
            {
                ++場風Index;
                Display局 = 1;
            }
            else
            {
                ++Display局;
            }

            if (ShouldFinish)
            {
                gameResult = new GameResult(this);
                return null;
            }
            gameResult = null;
            return Start局();
        }

        public AfterDraw? Advance局Byノーテン流局(out GameResult? gameResult)
        {
            return Advance局By子上がり(out gameResult);
        }


        public AfterDraw? Advance局By途中流局(out GameResult? gameResult)
        {
            return Advance局Byテンパイ流局(out gameResult);
        }

        public AfterDraw? Advance局Byテンパイ流局(out GameResult? gameResult)
        {
            ++本場;

            if (ShouldFinish)
            {
                gameResult = new GameResult(this);
                return null;
            }
            gameResult = null;
            return Start局();
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
                    || (連荘 == 0 && 本場 == 0 && Display局 == 1))
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
                    case Rules.LengthType.東風戦: return (場風Index == 0) && (Display局 == 4);
                    case Rules.LengthType.半荘戦: return (場風Index == 1) && (Display局 == 4);
                    case Rules.LengthType.一荘戦: return (場風Index == 3) && (Display局 == 4);
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
                    case Rules.LengthType.東風戦: return 場風Index > 0;
                    case Rules.LengthType.半荘戦: return 場風Index > 1;
                    case Rules.LengthType.一荘戦: return 場風Index > 3;
                    default:
                        throw new System.ArgumentException(rule.end.lengthType.ToString());
                }
            }
        }
    }
}

