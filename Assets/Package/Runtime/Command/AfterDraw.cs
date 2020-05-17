using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;

namespace TSKT.Mahjongs.Commands
{
    public abstract class CommandAfterDraw : ICommand
    {
        public readonly AfterDraw afterDraw;
        public readonly Player player;

        public CommandAfterDraw(Player player, AfterDraw afterDraw)
        {
            this.player = player;
            this.afterDraw = afterDraw;
        }

        public abstract CommandPriority Priority { get; }
        public abstract CommandResult TryExecute();
    }

    public class 九種九牌 : CommandAfterDraw
    {
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public override CommandPriority Priority => GetPriority;
        public 九種九牌(Player player, AfterDraw afterDraw) : base(player, afterDraw)
        {
        }

        public override CommandResult TryExecute()
        {
            var nextController = afterDraw.九種九牌(out var roundResult);
            var result = new CommandResult(nextController, roundResult);
            return result;
        }
    }

    public class Discard : CommandAfterDraw
    {
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public override CommandPriority Priority => GetPriority;
        readonly Tile tile;
        public Discard(AfterDraw afterDraw, Tile tile) : base(afterDraw.DrawPlayer, afterDraw)
        {
            this.tile = tile;
        }

        public override CommandResult TryExecute()
        {
            return new CommandResult(afterDraw.Discard(tile, false));
        }
    }
    public class Richi : CommandAfterDraw
    {
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public override CommandPriority Priority => GetPriority;
        readonly Tile tile;
        public Richi(AfterDraw afterDraw, Tile tile) : base(afterDraw.DrawPlayer, afterDraw)
        {
            this.tile = tile;
        }

        public override CommandResult TryExecute()
        {
            return new CommandResult(afterDraw.Discard(tile, true));
        }
    }
    public class DeclareClosedQuad : CommandAfterDraw
    {
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public override CommandPriority Priority => GetPriority;
        readonly TileType tile;
        public DeclareClosedQuad(AfterDraw afterDraw, TileType tile) : base(afterDraw.DrawPlayer, afterDraw)
        {
            this.tile = tile;
        }

        public override CommandResult TryExecute()
        {
            return new CommandResult(afterDraw.DeclareClosedQuad(tile));
        }
    }

    public class DeclareAddedOpenQuad : CommandAfterDraw
    {
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public override CommandPriority Priority => GetPriority;
        readonly Tile tile;
        public DeclareAddedOpenQuad(AfterDraw afterDraw, Tile tile) : base(afterDraw.DrawPlayer, afterDraw)
        {
            this.tile = tile;
        }

        public override CommandResult TryExecute()
        {
            return new CommandResult(afterDraw.DeclareAddedOpenQuad(tile));
        }
    }

    public class Tsumo : CommandAfterDraw
    {
        public static CommandPriority GetPriority => CommandPriority.Tsumo;
        public override CommandPriority Priority => GetPriority;
        public Tsumo(AfterDraw afterDraw) : base(afterDraw.DrawPlayer, afterDraw)
        {
        }

        public override CommandResult TryExecute()
        {
            var nextController = afterDraw.Tsumo(out var roundResult, out var completedResults);
            return new CommandResult(nextController, roundResult, completedResults);
        }
        public CompletedHand TsumoResult => afterDraw.tsumo.Value;
    }
}


