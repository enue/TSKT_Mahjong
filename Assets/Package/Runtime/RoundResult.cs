using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs
{
    public class RoundResult
    {
        readonly public GameResult gameResult;
        readonly public Dictionary<Player, int> scoreDiffs;
        readonly public Dictionary<Player, ExhausiveDrawType> states;

        public RoundResult(GameResult gameResult,
            Dictionary<Player, int> scoreDiffs = null,
            Dictionary<Player, ExhausiveDrawType> states = null)
        {
            this.gameResult = gameResult;
            this.scoreDiffs = scoreDiffs;
            this.states = states;
        }
    }
}
