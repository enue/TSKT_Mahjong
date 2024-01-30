#nullable enable
using System.Collections;
using System.Collections.Generic;
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
        public readonly (Tile, Tile) 対子;
        public readonly Tile TargetTile => Controller.捨て牌;

        public Pon(Player player, AfterDiscard afterDiscard, (Tile, Tile) 対子)
        {
            Controller = afterDiscard;
            Executor = player;
            this.対子 = 対子;
        }
        readonly public CommandResult Execute()
        {
            Controller.TryAttachFuriten();
            foreach (var it in Controller.局.players)
            {
                it.一発 = false;
            }

            var discardPile = Controller.DiscardPlayer.河;
            var tile = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            Executor.手牌.tiles.Remove(対子.Item1);
            Executor.手牌.tiles.Remove(対子.Item2);

            var meld = new 副露(
                (tile, Controller.DiscardPlayer.index),
                (対子.Item1, Executor.index),
                (対子.Item2, Executor.index));
            Executor.手牌.副露.Add(meld);

            Executor.OnTurnStart();
            return new CommandResult(new AfterDraw(Executor, null, 嶺上: false, openDoraAfterDiscard: false));
        }


        public (TileType, bool, TileType, bool, TileType, bool) Key => 副露Util.GetKey(TargetTile, 対子.Item1, 対子.Item2);
        public 副露 副露 => new((TargetTile, Controller.DiscardPlayer.index), (対子.Item1, Executor.index), (対子.Item2, Executor.index));
        public int RedTileCount
        {
            get
            {
                var result = 0;
                if (TargetTile.red)
                {
                    ++result;
                }
                if (対子.Item1.red)
                {
                    ++result;
                }
                if (対子.Item2.red)
                {
                    ++result;
                }
                return result;
            }
        }
    }

    public readonly struct Chi : ICommand<AfterDiscard>
    {
        public readonly AfterDiscard Controller { get; }
        public static CommandPriority GetPriority => CommandPriority.Chi;
        public readonly CommandPriority Priority => GetPriority;
        public readonly (Tile left, Tile right) 塔子;
        public readonly Tile TargetTile => Controller.捨て牌;
        public readonly Player Executor { get; }

        public Chi(Player player, AfterDiscard afterDiscard, (Tile left, Tile right) 塔子)
        {
            Controller = afterDiscard;
            Executor = player;
            this.塔子 = 塔子;
        }
        public readonly CommandResult Execute()
        {
            Controller.TryAttachFuriten();
            foreach (var it in Controller.局.players)
            {
                it.一発 = false;
            }

            var discardPile = Controller.DiscardPlayer.河;
            var tile = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            Executor.手牌.tiles.Remove(塔子.left);
            Executor.手牌.tiles.Remove(塔子.right);

            var meld = new 副露(
                (tile, Controller.DiscardPlayer.index),
                (塔子.left, Executor.index),
                (塔子.right, Executor.index));
            Executor.手牌.副露.Add(meld);

            Executor.OnTurnStart();

            return new CommandResult(new AfterDraw(Executor, null, 嶺上: false, openDoraAfterDiscard: false));
        }

        public (TileType, bool, TileType, bool, TileType, bool) Key => 副露Util.GetKey(TargetTile, 塔子.left, 塔子.right);
        public 副露 副露 => new((TargetTile, Controller.DiscardPlayer.index), (塔子.left, Executor.index), (塔子.right, Executor.index));

        public int RedTileCount
        {
            get
            {
                var result = 0;
                if (TargetTile.red)
                {
                    ++result;
                }
                if (塔子.Item1.red)
                {
                    ++result;
                }
                if (塔子.Item2.red)
                {
                    ++result;
                }
                return result;
            }
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
            Controller.TryAttachFuriten();
            var afterDraw = Controller.局.Execute大明槓(Executor, Controller.DiscardPlayer);
            return new CommandResult(afterDraw);
        }
    }
}
