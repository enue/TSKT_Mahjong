using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs.Rules
{
    public class EndRule
    {
        public LengthType lengthType = LengthType.東風戦;
        public bool suddenDeathInExtraRound = true;
        public int extraRoundScoreThreshold = 30000;
    }
}
