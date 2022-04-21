#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;
using TSKT.Mahjongs.Rounds;

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
            return Controller.Ron(Executor);
        }
        readonly public CompletedHand RonResult => Controller.PlayerRons[Executor];
    }
}
