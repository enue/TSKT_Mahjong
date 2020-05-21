using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;

namespace TSKT.Mahjongs.Commands
{
    public readonly struct Ron : ICommand<IRonableController>
    {
        public IRonableController Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Ron;
        public CommandPriority Priority => GetPriority;
        public Player Executor { get; }

        public Ron(Player player, IRonableController controller)
        {
            Controller = controller;
            Executor = player;
        }
        public CommandResult TryExecute()
        {
            return null;
        }
        public CompletedHand RonResult => Controller.PlayerRons[Executor];
    }
}
