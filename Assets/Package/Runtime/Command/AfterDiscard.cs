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
        public readonly Player player;

        public CommandAfterDiscard(Player player, AfterDiscard afterDiscard)
        {
            this.player = player;
            this.afterDiscard = afterDiscard;
        }

        public abstract CommandResult TryExecute();

        // ポンとカンが同時には発生しない
        // チーが二つは同時には発生しない
        // ので、優先順位比較はポンとチーだけ考えれば良い。
        public abstract CommandPriority Priority { get; }
    }
    public class Ron : CommandAfterDiscard
    {
        public static CommandPriority GetPriority => CommandPriority.Ron;
        public override CommandPriority Priority => GetPriority;
        public Ron(Player player, AfterDiscard afterDiscard) : base(player, afterDiscard)
        {
        }
        public override CommandResult TryExecute()
        {
            return null;
        }
        public CompletedHand RonResult => afterDiscard.rons[player];
    }

    public class Pon : CommandAfterDiscard
    {
        public static CommandPriority GetPriority => CommandPriority.Pon;
        public override CommandPriority Priority => GetPriority;
        readonly (Tile, Tile) pair;
        public Pon(Player player, AfterDiscard afterDiscard, (Tile, Tile) pair) : base(player, afterDiscard)
        {
            this.pair = pair;
        }
        public override CommandResult TryExecute()
        {
            return new CommandResult(afterDiscard.Pon(player, pair));
        }
    }

    public class Chi : CommandAfterDiscard
    {
        public static CommandPriority GetPriority => CommandPriority.Chi;
        public override CommandPriority Priority => GetPriority;
        readonly (Tile, Tile) 塔子;
        public Chi(Player player, AfterDiscard afterDiscard, (Tile, Tile) 塔子) : base(player, afterDiscard)
        {
            this.塔子 = 塔子;
        }
        public override CommandResult TryExecute()
        {
            return new CommandResult(afterDiscard.Chi(player, 塔子));
        }
    }

    public class Kan : CommandAfterDiscard
    {
        public static CommandPriority GetPriority => CommandPriority.Pon;
        public override CommandPriority Priority => GetPriority;
        public Kan(Player player, AfterDiscard afterDiscard) : base(player, afterDiscard)
        {
        }
        public override CommandResult TryExecute()
        {
            return new CommandResult(afterDiscard.OpenQuad(player));
        }
    }
}
