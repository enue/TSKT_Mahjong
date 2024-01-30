#nullable enable
using System.Collections;
using System.Collections.Generic;
using TSKT;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs.Commands
{
    public readonly struct Ron : ICommand<IController>
    {
        public readonly IController Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Ron;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor { get; }

        public readonly 和了 RonResult { get; }
        public Ron(Player player, IController controller, 和了 completedHand)
        {
            Controller = controller;
            Executor = player;
            RonResult = completedHand;
        }
        public readonly CommandResult Execute()
        {
            return 和了.Execute((Executor, RonResult));
        }
    }
}
