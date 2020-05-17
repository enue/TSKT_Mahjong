using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public interface IController
    {
        Round Round { get; }
        bool Consumed { get; }
        bool CanRon(Player player);
        AfterDraw Ron(out RoundResult roundResult, out Dictionary<Player, CompletedResult> result, params Player[] players);

        IController DoDefaultAction(out RoundResult roundResult);
    }

    public interface IBeforeQuad : IController
    {
        Player DeclarePlayer { get; }
        AfterDraw BuildQuad();
    }
}
