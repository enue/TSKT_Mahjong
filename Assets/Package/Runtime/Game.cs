using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class Game
    {
        readonly TileType[] winds = new[] { TileType.東, TileType.南, TileType.西, TileType.北 };
        public int RoundWindCount { get; private set; } = 0;
        public TileType Wind => winds[RoundWindCount % winds.Length];
        public int DisplayRoundCount { get; private set; } = 1;
        public int riichiScore;
        public int 本場 { get; private set; }
        public int 連荘 { get; private set; }
        public readonly int firstDealer;
        public readonly ScoreOwner[] scoreOwners;
        public readonly RuleSetting rule;

        static public BeforeRoundStart Create(int firstDealer, RuleSetting rule)
        {
            var game = new Game(firstDealer, rule);
            return new BeforeRoundStart(game);
        }

        Game(int firstDealer, RuleSetting rule)
        {
            this.rule = rule;
            this.firstDealer = firstDealer;
            scoreOwners = new ScoreOwner[4];
            for (int i = 0; i < scoreOwners.Length; ++i)
            {
                scoreOwners[i] = new ScoreOwner(this);
            }
        }

        public AfterDraw StartRound(params TileType[][] initialPlayerTilesByCheat)
        {
            var dealer = (firstDealer + DisplayRoundCount - 1) % 4;
            var round = new Round(this, Wind, dealer, initialPlayerTilesByCheat);
            return round.Start();
        }

        public RoundResult AdvanceRoundBy親上がり()
        {
            ++連荘;
            ++本場;

            if (ShouldFinish)
            {
                return new RoundResult(new GameResult(this));
            }
            return new RoundResult(new BeforeRoundStart(this));
        }
        public RoundResult AdvanceRoundBy子上がり()
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
                return new RoundResult(new GameResult(this));
            }
            return new RoundResult(new BeforeRoundStart(this));
        }

        public RoundResult AdvanceRoundByノーテン流局()
        {
            return AdvanceRoundBy子上がり();
        }

        public RoundResult AdvanceRoundByテンパイ流局()
        {
            ++本場;

            if (ShouldFinish)
            {
                return new RoundResult(new GameResult(this));
            }
            return new RoundResult(new BeforeRoundStart(this));
        }


        bool ShouldFinish
        {
            get
            {
                if (rule.end.endWhenScoreUnderZero)
                {
                    if (scoreOwners.Select(_ => _.score).Min() < 0)
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
                    var topScore = scoreOwners.Max(_ => _.score);
                    if (topScore >= rule.end.extraRoundScoreThreshold)
                    {
                        return true;
                    }
                }

                return false;
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

