using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;

namespace TSKT.Mahjongs.Commands
{
    public abstract class CommandAfterDiscard : ICommand
    {
        public readonly AfterDiscard afterDiscard;

        public CommandAfterDiscard(AfterDiscard afterDiscard)
        {
            this.afterDiscard = afterDiscard;
        }

        public abstract CommandResult TryExecute();
        // ポンとカンが同時には発生しない
        // チーが二つは同時には発生しない
        // ので、優先順位比較はポンとチーだけ考えれば良い。
        public abstract CommandPriority Priority { get; }
        public abstract Player Executor { get; }
    }

    public class Ron : CommandAfterDiscard
    {
        public static CommandPriority GetPriority => CommandPriority.Ron;
        public override CommandPriority Priority => GetPriority;
        public override Player Executor { get; }

        public Ron(Player player, AfterDiscard afterDiscard) : base(afterDiscard)
        {
            Executor = player;
        }
        public override CommandResult TryExecute()
        {
            return null;
        }
        public CompletedHand RonResult => afterDiscard.PlayerRons[Executor];
    }

    public class Pon : CommandAfterDiscard
    {
        public static CommandPriority GetPriority => CommandPriority.Pon;
        public override CommandPriority Priority => GetPriority;
        public override Player Executor { get; }

        readonly (Tile, Tile) pair;

        public Pon(Player player, AfterDiscard afterDiscard, (Tile, Tile) pair) : base(afterDiscard)
        {
            Executor = player;
            this.pair = pair;
        }
        public override CommandResult TryExecute()
        {
            return new CommandResult(afterDiscard.Pon(Executor, pair));
        }
    }

    public class Chi : CommandAfterDiscard
    {
        public static CommandPriority GetPriority => CommandPriority.Chi;
        public override CommandPriority Priority => GetPriority;
        readonly (Tile, Tile) 塔子;
        public override Player Executor { get; }

        public Chi(Player player, AfterDiscard afterDiscard, (Tile, Tile) 塔子) : base(afterDiscard)
        {
            Executor = player;
            this.塔子 = 塔子;
        }
        public override CommandResult TryExecute()
        {
            return new CommandResult(afterDiscard.Chi(Executor, 塔子));
        }
    }

    public class Kan : CommandAfterDiscard
    {
        public static CommandPriority GetPriority => CommandPriority.Pon;
        public override CommandPriority Priority => GetPriority;
        public override Player Executor { get; }

        public Kan(Player player, AfterDiscard afterDiscard) : base(afterDiscard)
        {
            Executor = player;
        }
        public override CommandResult TryExecute()
        {
            return new CommandResult(afterDiscard.OpenQuad(Executor));
        }
    }
}
