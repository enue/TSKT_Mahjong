#nullable enable
using System.Collections;
using System.Collections.Generic;
using TSKT.Mahjongs.Rounds;
using UnityEngine;

namespace TSKT.Mahjongs.ScriptableRules
{
    public interface ICompletedResultModifier
    {
        void Modify(ref CompletedResult source);
    }
}
