#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs.Commands
{
    public readonly struct 九種九牌 : ICommand<AfterDraw>
    {
        public readonly AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor => Controller.DrawPlayer;

        public 九種九牌(AfterDraw afterDraw)
        {
            Controller = afterDraw;
        }

        readonly public CommandResult Execute()
        {
            var nextController = Controller.九種九牌(out var roundResult);
            return new CommandResult(nextController, roundResult);
        }
    }

    public readonly struct Discard : ICommand<AfterDraw>
    {
        public readonly AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor => Controller.DrawPlayer;
        public readonly Tile tile;
        public readonly bool riichi;
        public readonly bool openRiichi;

        public Discard(AfterDraw afterDraw, Tile tile, bool riichi, bool openRiichi)
        {
            Controller = afterDraw;
            this.tile = tile;
            this.riichi = riichi;
            this.openRiichi = openRiichi;
        }

        readonly public bool Furiten
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

        readonly public TileType[] WinningTiles => HandAfterDiscard.GetWinningTiles();

        readonly public Dictionary<TileType, int> WinningTilesHiddenCount
        {
            get
            {
                var controller = Controller;
                var executor = Executor;
                return WinningTiles.ToDictionary(_ => _, _ => controller.Round.HiddenTileCountFrom(executor, _));
            }
        }

        readonly public TileType[] TilesToShowWhenOpenRiichi
        {
            get
            {
                var result = new List<TileType>();

                var winningTiles = WinningTiles;
                foreach (var winningTile in winningTiles)
                {
                    var cloneHand = HandAfterDiscard;
                    cloneHand.tiles.Add(new Tile(-1, winningTile, false));
                    var solution = cloneHand.Solve();
                    foreach (var structure in solution.structures)
                    {
                        // IsolatedTilesがあるアガリは国士なので手牌全てが関係牌
                        if (structure.IsolatedTiles.Length != 0)
                        {
                            return HandAfterDiscard.tiles.Select(_ => _.type).Distinct().ToArray();
                        }

                        foreach (var set in structure.Sets)
                        {
                            if (set.first == winningTile)
                            {
                                result.Add(set.second);
                                result.Add(set.third);
                            }
                            else if (set.second == winningTile)
                            {
                                result.Add(set.first);
                                result.Add(set.third);
                            }
                            else if (set.third == winningTile)
                            {
                                result.Add(set.first);
                                result.Add(set.second);
                            }
                        }
                        foreach (var pair in structure.Pairs)
                        {
                            if (pair == winningTile)
                            {
                                result.Add(winningTile);
                            }
                        }
                    }
                }

                return result.Distinct().ToArray();
            }
        }

        readonly public Hand HandAfterDiscard
        {
            get
            {
                var cloneHand = Controller.DrawPlayer.hand.Clone();
                cloneHand.tiles.Remove(tile);
                return cloneHand;
            }
        }

        readonly public CommandResult Execute()
        {
            return new CommandResult(Controller.Discard(tile, riichi, openRiichi));
        }
    }

    /// <summary>
    /// 暗槓
    /// </summary>
    public readonly struct DeclareClosedQuad : ICommand<AfterDraw>
    {
        public readonly AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor => Controller.DrawPlayer;
        public readonly TileType tile;

        public DeclareClosedQuad(AfterDraw afterDraw, TileType tile)
        {
            Controller = afterDraw;
            this.tile = tile;
        }

        readonly public CommandResult Execute()
        {
            return new CommandResult(Controller.DeclareClosedQuad(tile));
        }
    }

    /// <summary>
    /// 加槓
    /// </summary>
    public readonly struct DeclareAddedOpenQuad : ICommand<AfterDraw>
    {
        public readonly AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor => Controller.DrawPlayer;
        public readonly Tile tile;

        public DeclareAddedOpenQuad(AfterDraw afterDraw, Tile tile)
        {
            Controller =afterDraw;
            this.tile = tile;
        }

        readonly public CommandResult Execute()
        {
            return new CommandResult(Controller.DeclareAddedOpenQuad(tile));
        }
    }

    public readonly struct Tsumo : ICommand<AfterDraw>
    {
        public readonly AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Tsumo;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor => Controller.DrawPlayer;

        public Tsumo(AfterDraw afterDraw)
        {
            Controller = afterDraw;
        }

        readonly public CommandResult Execute()
        {
            var nextController = Controller.Tsumo(out var roundResult, out var completedResults);
            return new CommandResult(nextController, roundResult, completedResults);
        }
    }
}


