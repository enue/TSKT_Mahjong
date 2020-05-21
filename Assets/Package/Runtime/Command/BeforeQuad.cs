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
}

