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

        public CommandAfterDraw(AfterDraw afterDraw)
        {
            this.afterDraw = afterDraw;
        }

        public abstract CommandPriority Priority { get; }
        public abstract CommandResult TryExecute();
        public abstract Player Executor { get; }
    }

    public class 九種九牌 : CommandAfterDraw
    {
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public override CommandPriority Priority => GetPriority;
        public override Player Executor => afterDraw.DrawPlayer;

        public 九種九牌(AfterDraw afterDraw) : base(afterDraw)
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
        public override Player Executor => afterDraw.DrawPlayer;
        public readonly Tile tile;
        public readonly bool riichi;

        TileType[] winningTiles;
        public TileType[] WinningTiles
        {
            get
            {
                if (winningTiles == null)
                {
                    var hand = afterDraw.DrawPlayer.hand.Clone();
                    hand.tiles.Remove(tile);
                    winningTiles = hand.GetWinningTiles();
                }
                return winningTiles;
            }
        }

        public Discard(AfterDraw afterDraw, Tile tile, bool riichi) : base(afterDraw)
        {
            this.tile = tile;
            this.riichi = riichi;
        }

        public bool Furiten
        {
            get
            {
                if (WinningTiles.Length == 0)
                {
                    return false;
                }

                if (WinningTiles.Contains(tile.type))
                {
                    return true;
                }
                var discardedTiles = Executor.discardedTiles.Select(_ => _.type).Distinct();
                foreach (var it in discardedTiles)
                {
                    if (WinningTiles.Contains(it))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override CommandResult TryExecute()
        {
            return new CommandResult(afterDraw.Discard(tile, riichi));
        }
    }

    public class DeclareClosedQuad : CommandAfterDraw
    {
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public override CommandPriority Priority => GetPriority;
        public override Player Executor => afterDraw.DrawPlayer;
        readonly TileType tile;

        public DeclareClosedQuad(AfterDraw afterDraw, TileType tile) : base(afterDraw)
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
        public override Player Executor => afterDraw.DrawPlayer;
        readonly Tile tile;

        public DeclareAddedOpenQuad(AfterDraw afterDraw, Tile tile) : base(afterDraw)
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
        public override Player Executor => afterDraw.DrawPlayer;

        public Tsumo(AfterDraw afterDraw) : base(afterDraw)
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


