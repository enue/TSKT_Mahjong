using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;

namespace TSKT.Mahjongs.Commands
{
    public readonly struct 九種九牌 : ICommand<AfterDraw>
    {
        public AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public CommandPriority Priority => GetPriority;
        public Player Executor => Controller.DrawPlayer;

        public 九種九牌(AfterDraw afterDraw)
        {
            Controller = afterDraw;
        }

        public CommandResult TryExecute()
        {
            var nextController = Controller.九種九牌(out var roundResult);
            var result = new CommandResult(nextController, roundResult);
            return result;
        }
    }

    public readonly struct Discard : ICommand<AfterDraw>
    {
        public AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public CommandPriority Priority => GetPriority;
        public Player Executor => Controller.DrawPlayer;
        public readonly Tile tile;
        public readonly bool riichi;

        public Discard(AfterDraw afterDraw, Tile tile, bool riichi)
        {
            Controller = afterDraw;
            this.tile = tile;
            this.riichi = riichi;
        }

        public bool Furiten
        {
            get
            {
                var winningTiles = WinningTiles;
                if (winningTiles.Length == 0)
                {
                    return false;
                }

                if (winningTiles.Contains(tile.type))
                {
                    return true;
                }
                var discardedTiles = Executor.discardedTiles.Select(_ => _.type).Distinct();
                foreach (var it in discardedTiles)
                {
                    if (winningTiles.Contains(it))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public TileType[] WinningTiles
        {
            get
            {
                var cloneHand = Controller.DrawPlayer.hand.Clone();
                cloneHand.tiles.Remove(tile);
                return cloneHand.GetWinningTiles();
            }
        }

        public CommandResult TryExecute()
        {
            return new CommandResult(Controller.Discard(tile, riichi));
        }
    }

    public readonly struct DeclareClosedQuad : ICommand<AfterDraw>
    {
        public AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public CommandPriority Priority => GetPriority;
        public Player Executor => Controller.DrawPlayer;
        public readonly TileType tile;

        public DeclareClosedQuad(AfterDraw afterDraw, TileType tile)
        {
            Controller = afterDraw;
            this.tile = tile;
        }

        public CommandResult TryExecute()
        {
            return new CommandResult(Controller.DeclareClosedQuad(tile));
        }
    }

    public readonly struct DeclareAddedOpenQuad : ICommand<AfterDraw>
    {
        public AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public CommandPriority Priority => GetPriority;
        public Player Executor => Controller.DrawPlayer;
        public readonly Tile tile;

        public DeclareAddedOpenQuad(AfterDraw afterDraw, Tile tile)
        {
            Controller =afterDraw;
            this.tile = tile;
        }

        public CommandResult TryExecute()
        {
            return new CommandResult(Controller.DeclareAddedOpenQuad(tile));
        }
    }

    public readonly struct Tsumo : ICommand<AfterDraw>
    {
        public AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Tsumo;
        public CommandPriority Priority => GetPriority;
        public Player Executor => Controller.DrawPlayer;

        public Tsumo(AfterDraw afterDraw)
        {
            Controller = afterDraw;
        }

        public CommandResult TryExecute()
        {
            var nextController = Controller.Tsumo(out var roundResult, out var completedResults);
            return new CommandResult(nextController, roundResult, completedResults);
        }
        public CompletedHand TsumoResult => Controller.tsumo.Value;
    }
}


