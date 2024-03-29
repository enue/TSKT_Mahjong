﻿#nullable enable
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
            Controller.TryAttachFuriten();
            foreach (var it in Controller.Round.players)
            {
                it.一発 = false;
            }

            var discardPile = Controller.DiscardPlayer.discardPile;
            var tile = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            Executor.hand.tiles.Remove(pair.Item1);
            Executor.hand.tiles.Remove(pair.Item2);

            var meld = new Meld(
                (tile, Controller.DiscardPlayer.index),
                (pair.Item1, Executor.index),
                (pair.Item2, Executor.index));
            Executor.hand.melds.Add(meld);

            Executor.OnTurnStart();
            return new CommandResult(new AfterDraw(Executor, null, 嶺上: false, openDoraAfterDiscard: false));
        }


        public (TileType, bool, TileType, bool, TileType, bool) Key => MeldUtil.GetKey(TargetTile, pair.Item1, pair.Item2);
        public Meld Meld => new Meld((TargetTile, Controller.DiscardPlayer.index), (pair.Item1, Executor.index), (pair.Item2, Executor.index));
        public int RedTileCount
        {
            get
            {
                var result = 0;
                if (TargetTile.red)
                {
                    ++result;
                }
                if (pair.Item1.red)
                {
                    ++result;
                }
                if (pair.Item2.red)
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
        public readonly Tile TargetTile => Controller.DiscardedTile;
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
            foreach (var it in Controller.Round.players)
            {
                it.一発 = false;
            }

            var discardPile = Controller.DiscardPlayer.discardPile;
            var tile = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            Executor.hand.tiles.Remove(塔子.Item1);
            Executor.hand.tiles.Remove(塔子.Item2);

            var meld = new Meld(
                (tile, Controller.DiscardPlayer.index),
                (塔子.Item1, Executor.index),
                (塔子.Item2, Executor.index));
            Executor.hand.melds.Add(meld);

            Executor.OnTurnStart();

            return new CommandResult(new AfterDraw(Executor, null, 嶺上: false, openDoraAfterDiscard: false));
        }

        public (TileType, bool, TileType, bool, TileType, bool) Key => MeldUtil.GetKey(TargetTile, 塔子.left, 塔子.right);
        public Meld Meld => new Meld((TargetTile, Controller.DiscardPlayer.index), (塔子.left, Executor.index), (塔子.right, Executor.index));

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
            var afterDraw = Controller.Round.ExecuteOpenQuad(Executor, Controller.DiscardPlayer);
            return new CommandResult(afterDraw);
        }
    }
}
