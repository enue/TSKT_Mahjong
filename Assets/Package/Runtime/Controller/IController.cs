using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#nullable enable

namespace TSKT.Mahjongs
{
    public interface IController
    {
        Round Round { get; }
        bool Consumed { get; }

        IController? DoDefaultAction(out RoundResult? roundResult);
        ICommand[] ExecutableCommands { get; }
        CommandResult ExecuteCommands(out List<ICommand> executedCommands, params ICommand[] commands);
        Serializables.Session SerializeSession();
    }

    public interface IRonableController : IController
    {
        bool CanRon(out Commands.Ron[] command);
        bool CanRon(Player player, out Commands.Ron command);
        AfterDraw? Ron(out RoundResult roundResult, out Dictionary<Player, CompletedResult> result, params Player[] players);
        Dictionary<Player, CompletedHand> PlayerRons { get; }
    }

    public interface IBeforeQuad : IController, IRonableController
    {
        Player DeclarePlayer { get; }
        AfterDraw BuildQuad();
    }
}
