using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;
#nullable enable

namespace TSKT.Mahjongs.Commands
{
    public readonly struct Ron : ICommand<IRonableController>
    {
        public readonly IRonableController Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Ron;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor { get; }

        public Ron(Player player, IRonableController controller)
        {
            Controller = controller;
            Executor = player;
        }
        readonly public CommandResult Execute()
        {
            var nextController = Controller.Ron(out var roundResult, out var completedResults, Executor);
            return new CommandResult(nextController, roundResult, completedResults);
        }
        readonly public CompletedHand RonResult => Controller.PlayerRons[Executor];
    }
}
