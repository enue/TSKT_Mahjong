#nullable enable
using System.Collections;
using System.Collections.Generic;

namespace TSKT.Mahjongs.Rounds
{
    public class 局Result
    {
        readonly public GameResult? gameResult;
        readonly public Dictionary<Player, int>? scoreDiffs;
        readonly public Dictionary<Player, ExhausiveDrawType>? states;

        public 局Result(GameResult? gameResult,
            Dictionary<Player, int>? scoreDiffs = null,
            Dictionary<Player, ExhausiveDrawType>? states = null)
        {
            this.gameResult = gameResult;
            this.scoreDiffs = scoreDiffs;
            this.states = states;
        }
    }
}
