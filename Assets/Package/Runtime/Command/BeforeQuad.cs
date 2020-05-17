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

        public CommandBeforeQuad(IBeforeQuad controller)
        {
            this.controller = controller;
        }

        public abstract CommandResult TryExecute();
        public abstract CommandPriority Priority { get; }
        public abstract Player Executor { get; }
    }

    public class 槍槓 : CommandBeforeQuad
    {
        public static CommandPriority GetPriority => CommandPriority.Ron;
        public override CommandPriority Priority => GetPriority;
        public override Player Executor { get; }

        public 槍槓(Player player, IBeforeQuad controller) : base(controller)
        {
            Executor = player;
        }

        public override CommandResult TryExecute()
        {
            return null;
        }
        public CompletedHand RonResult => controller.PlayerRons[Executor];
    }
}

