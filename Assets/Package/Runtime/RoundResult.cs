using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs
{
    public class RoundResult
    {
        readonly public GameResult gameResult;
        public Dictionary<Player, int> scoreDiffs;
        public Dictionary<Player, ExhausiveDrawType> states;

        public RoundResult(GameResult gameResult)
        {
            this.gameResult = gameResult;
        }
    }
}
