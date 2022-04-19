#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs.Commands
{
    public readonly struct Pon : ICommand<AfterDiscard>
    {
        public readonly AfterDiscard Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Pon;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor { get; }
        public readonly (Tile, Tile) pair;
        public readonly Tile TargetTile => Controller.DiscardedTile;

        public Pon(Player player, AfterDiscard afterDiscard, (Tile, Tile) pair)
        {
            Controller = afterDiscard;
            Executor = player;
            this.pair = pair;
        }
        readonly public CommandResult Execute()
        {
            return new CommandResult(Controller.Pon(Executor, pair));
        }
    }

    public readonly struct Chi : ICommand<AfterDiscard>
    {
        public readonly AfterDiscard Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Chi;
        public readonly CommandPriority Priority => GetPriority;
        public readonly (Tile left, Tile right) 塔子;
        public readonly Tile TargetTile => Controller.DiscardedTile;
        public readonly Player Executor { get; }

        public Chi(Player player, AfterDiscard afterDiscard, (Tile left, Tile right) 塔子)
        {
            Controller = afterDiscard;
            Executor = player;
            this.塔子 = 塔子;
        }
        readonly public CommandResult Execute()
        {
            return new CommandResult(Controller.Chi(Executor, 塔子));
        }
    }

    public readonly struct Kan : ICommand<AfterDiscard>
    {
        public readonly AfterDiscard Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Pon;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor { get; }

        public Kan(Player player, AfterDiscard afterDiscard)
        {
            Controller = afterDiscard;
            Executor = player;
        }
        readonly public CommandResult Execute()
        {
            return new CommandResult(Controller.OpenQuad(Executor));
        }
    }
}
