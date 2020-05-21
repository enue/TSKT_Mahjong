using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;

namespace TSKT.Mahjongs.Commands
{
    public readonly struct Pon : ICommand<AfterDiscard>
    {
        public AfterDiscard Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Pon;
        public CommandPriority Priority => GetPriority;
        public Player Executor { get; }
        public readonly (Tile, Tile) pair;

        public Pon(Player player, AfterDiscard afterDiscard, (Tile, Tile) pair)
        {
            Controller = afterDiscard;
            Executor = player;
            this.pair = pair;
        }
        public CommandResult TryExecute()
        {
            return new CommandResult(Controller.Pon(Executor, pair));
        }
    }

    public readonly struct Chi : ICommand<AfterDiscard>
    {
        public AfterDiscard Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Chi;
        public CommandPriority Priority => GetPriority;
        public readonly (Tile left, Tile right) 塔子;
        public Player Executor { get; }

        public Chi(Player player, AfterDiscard afterDiscard, (Tile left, Tile right) 塔子)
        {
            Controller = afterDiscard;
            Executor = player;
            this.塔子 = 塔子;
        }
        public CommandResult TryExecute()
        {
            return new CommandResult(Controller.Chi(Executor, 塔子));
        }
    }

    public readonly struct Kan : ICommand<AfterDiscard>
    {
        public AfterDiscard Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Pon;
        public CommandPriority Priority => GetPriority;
        public Player Executor { get; }

        public Kan(Player player, AfterDiscard afterDiscard)
        {
            Controller = afterDiscard;
            Executor = player;
        }
        public CommandResult TryExecute()
        {
            return new CommandResult(Controller.OpenQuad(Executor));
        }
    }
}
