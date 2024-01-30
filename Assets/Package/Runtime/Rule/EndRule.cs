using System.Collections;
using System.Collections.Generic;
#nullable enable

namespace TSKT.Mahjongs.Rules
{
    [System.Serializable]
    public class EndRule
    {
        public LengthType lengthType = LengthType.東風戦;
        public bool endWhenScoreUnderZero = true;
        public bool suddenDeathInExtraRound = true;
        public int extraRoundScoreThreshold = 30000;
        public アガリ止め アガリ止め = アガリ止め.なし;
    }
}
