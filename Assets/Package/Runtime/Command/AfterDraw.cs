#nullable enable
using System.Collections;
using System.Collections.Generic;
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

        public readonly CommandResult Execute()
        {
            var result = Controller.局.game.Advance局By途中流局(out var gameResult);
            var roundResult = new 局Result(gameResult);
            return new CommandResult(result, roundResult);
        }
    }

    public readonly struct Discard : ICommand<AfterDraw>
    {
        public readonly AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor => Controller.DrawPlayer;
        public readonly Tile tile;
        public readonly bool リーチ;
        public readonly bool オープンリーチ;

        public Discard(AfterDraw afterDraw, Tile tile, bool リーチ, bool オープンリーチ)
        {
            Controller = afterDraw;
            this.tile = tile;
            this.リーチ = リーチ;
            this.オープンリーチ = オープンリーチ;
        }

        public readonly bool フリテン
        {
            get
            {
                var winningTiles = 和了牌;
                if (winningTiles.Length == 0)
                {
                    return false;
                }

                if (winningTiles.Contains(tile.type))
                {
                    return true;
                }
                var discardedTiles = Executor.捨て牌.Select(_ => _.type).Distinct();
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

        readonly public TileType[] 和了牌 => HandAfterDiscard.Get和了牌();

        readonly public Dictionary<TileType, int> WinningTilesHiddenCount
        {
            get
            {
                var controller = Controller;
                var executor = Executor;
                return 和了牌.ToDictionary(_ => _, _ => controller.局.HiddenTileCountFrom(executor, _));
            }
        }

        readonly public TileType[] TilesToShowWhenOpenRiichi
        {
            get
            {
                var result = new List<TileType>();

                var winningTiles = 和了牌;
                foreach (var winningTile in winningTiles)
                {
                    var cloneHand = HandAfterDiscard;
                    cloneHand.tiles.Add(new Tile(-1, winningTile, false));
                    var solution = cloneHand.Solve();
                    foreach (var structure in solution.structures)
                    {
                        // IsolatedTilesがあるアガリは国士なので手牌全てが関係牌
                        if (structure.浮き牌.Length != 0)
                        {
                            return HandAfterDiscard.tiles.Select(_ => _.type).Distinct().ToArray();
                        }

                        foreach (var set in structure.面子s)
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
                        foreach (var pair in structure.対子)
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

        readonly public 手牌 HandAfterDiscard
        {
            get
            {
                var cloneHand = Controller.DrawPlayer.手牌.Clone();
                cloneHand.tiles.Remove(tile);
                return cloneHand;
            }
        }

        public readonly CommandResult Execute()
        {
            Controller.DrawPlayer.Discard(tile, リーチ, オープンリーチ);
            if (Controller.openDoraAfterDiscard)
            {
                Controller.DrawPlayer.局.王牌.OpenDora();
            }

            return new CommandResult(new AfterDiscard(Controller.DrawPlayer));
        }
    }

    public readonly struct 暗槓 : ICommand<AfterDraw>
    {
        public readonly AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor => Controller.DrawPlayer;
        public readonly TileType tile;

        public 暗槓(AfterDraw afterDraw, TileType tile)
        {
            Controller = afterDraw;
            this.tile = tile;
        }

        public readonly CommandResult Execute()
        {
            if (Controller.openDoraAfterDiscard)
            {
                Controller.DrawPlayer.局.王牌.OpenDora();
            }
            return new CommandResult(new Before暗槓(Controller.DrawPlayer, tile));
        }
    }

    public readonly struct 加槓 : ICommand<AfterDraw>
    {
        public readonly AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Lowest;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor => Controller.DrawPlayer;
        public readonly Tile tile;

        public 加槓(AfterDraw afterDraw, Tile tile)
        {
            Controller = afterDraw;
            this.tile = tile;
        }

        public readonly CommandResult Execute()
        {
            if (Controller.openDoraAfterDiscard)
            {
                Controller.DrawPlayer.局.王牌.OpenDora();
            }
            return new CommandResult(new Before加槓(Controller.DrawPlayer, tile));
        }
    }

    public readonly struct ツモ上がり : ICommand<AfterDraw>
    {
        public readonly AfterDraw Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Tsumo;
        public readonly CommandPriority Priority => GetPriority;
        public readonly Player Executor => Controller.DrawPlayer;

        public ツモ上がり(AfterDraw afterDraw)
        {
            Controller = afterDraw;
        }

        public readonly CommandResult Execute()
        {
            // 明槓だとここでドラが増えるので点数の確定もここでおこなう
            if (Controller.openDoraAfterDiscard)
            {
                Controller.局.王牌.OpenDora();
            }
            return 和了.Execute((Controller.DrawPlayer, Controller.和了));
        }
    }
}


