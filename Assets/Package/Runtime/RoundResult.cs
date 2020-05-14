using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs
{
    public class RoundResult
    {
        readonly public GameResult gameResult;
        readonly public BeforeRoundStart beforeRoundStart;
        public Dictionary<Player, int> scoreDiffs;

        public RoundResult(GameResult gameResult)
        {
            this.gameResult = gameResult;
        }
        public RoundResult(BeforeRoundStart beforeRoundStart)
        {
            this.beforeRoundStart = beforeRoundStart;
        }
    }
}
