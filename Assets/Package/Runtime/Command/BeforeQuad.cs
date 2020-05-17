using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;

namespace TSKT.Mahjongs.Commands
{
    public abstract class CommandBeforeQuad : ICommand
    {
        public readonly IBeforeQuad controller;
        public readonly Player player;

        public CommandBeforeQuad(Player player, IBeforeQuad controller)
        {
            this.player = player;
            this.controller = controller;
        }

        public abstract CommandResult TryExecute();
        public abstract CommandPriority Priority { get; }
    }

    public class ExecuteQuad : CommandBeforeQuad
    {
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public override CommandPriority Priority => GetPriority;
        public ExecuteQuad(IBeforeQuad controller) : base(controller.DeclarePlayer, controller)
        {
        }

        public override CommandResult TryExecute()
        {
            return new CommandResult(controller.BuildQuad());
        }
    }

    public class 槍槓 : CommandBeforeQuad
    {
        public static CommandPriority GetPriority => CommandPriority.Ron;
        public override CommandPriority Priority => GetPriority;
        public 槍槓(Player player, IBeforeQuad controller) : base(player, controller)
        {
        }

        public override CommandResult TryExecute()
        {
            return null;
        }
    }
}

