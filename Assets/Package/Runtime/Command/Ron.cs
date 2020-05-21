using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;

namespace TSKT.Mahjongs.Commands
{
    public class Ron : ICommand
    {
        public static CommandPriority GetPriority => CommandPriority.Ron;
        public CommandPriority Priority => GetPriority;
        public readonly IRonableController controller;
        public Player Executor { get; }

        public Ron(Player player, IRonableController controller)
        {
            this.controller = controller;
            Executor = player;
        }
        public CommandResult TryExecute()
        {
            return null;
        }
        public CompletedHand RonResult => controller.PlayerRons[Executor];
    }
}
