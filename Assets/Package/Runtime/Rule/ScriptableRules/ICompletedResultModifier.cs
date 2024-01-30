#nullable enable
using System.Collections;
using System.Collections.Generic;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs.ScriptableRules
{
    public interface ICompletedResultModifier
    {
        void Modify(ref CompletedResult source);
    }
}
