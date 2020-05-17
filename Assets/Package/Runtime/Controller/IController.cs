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

        IController DoDefaultAction(out RoundResult roundResult);
        ICommand[] ExecutableCommands { get; }
    }

    public interface IRonableController
    {
        bool CanRon(Player player);
        AfterDraw Ron(out RoundResult roundResult, out Dictionary<Player, CompletedResult> result, params Player[] players);
        Dictionary<Player, CompletedHand> PlayerRons { get; }
    }

    public interface IBeforeQuad : IController, IRonableController
    {
        Player DeclarePlayer { get; }
        AfterDraw BuildQuad();
    }
}
