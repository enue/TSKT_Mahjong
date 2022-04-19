#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs
{
    public interface IController
    {
        Round Round { get; }
        bool Consumed { get; }

        AfterDraw? DoDefaultAction(out RoundResult? roundResult);
        ICommand[] ExecutableCommands { get; }
        void GetExecutableCommands(
            out Commands.Ron[] rons,
            out Commands.Chi[] chies,
            out Commands.Pon[] pons,
            out Commands.Kan[] kans,
            out Commands.DeclareClosedQuad[] declareCloseQuads,
            out Commands.DeclareAddedOpenQuad[] declareAddedOpenQuads,
            out Commands.Discard[] discards,
            out Commands.Discard[] riichies,
            out Commands.Discard[] openRiichies,
            out Commands.Tsumo? tsumo,
            out Commands.九種九牌? nineTiles);

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
