using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable

namespace TSKT.Mahjongs.Hands
{
    public readonly struct Structure
    {
        public readonly struct Set
        {
            public readonly TileType first;
            public readonly TileType second;
            public readonly TileType third;

            public Set(TileType first, TileType second, TileType third)
            {
                this.first = first;
                this.second = second;
                this.third = third;
            }

            readonly public bool 刻子 => first == second;
            readonly public bool 順子 => !刻子;
        }

        readonly TileType[] unsolvedTiles;
        public TileType[] IsolatedTiles { get; }
        public Set[] Sets { get; }
        public TileType[] Pairs { get; }
        public (TileType left, TileType right)[] 塔子 { get; }
        public readonly Meld[] melds;
        public readonly int 国士無双向聴数;
        public readonly int 七対子向聴数;
        public readonly int redTileCount;

        Structure(Structure source, TileType[] unsolvedTiles, TileType[]? isolatedTiles = null, Set[]? sets = null, TileType[]? pairs = null, (TileType left, TileType right)[]? 塔子 = null)
        {
            this.unsolvedTiles = unsolvedTiles;
            IsolatedTiles = isolatedTiles ?? source.IsolatedTiles;
            Sets = sets ?? source.Sets;
            Pairs = pairs ?? source.Pairs;
            this.塔子 = 塔子 ?? source.塔子;

            melds = source.melds;
            国士無双向聴数 = source.国士無双向聴数;
            七対子向聴数 = source.七対子向聴数;
            redTileCount = source.redTileCount;
        }
        public Structure(Hand hand)
        {
            IsolatedTiles = System.Array.Empty<TileType>();
            Sets = System.Array.Empty<Set>();
            Pairs = System.Array.Empty<TileType>();
            塔子 = System.Array.Empty<(TileType, TileType)>();

            unsolvedTiles = hand.tiles.Select(_ => _.type).ToArray();
            System.Array.Sort(unsolvedTiles);
            melds = hand.melds.ToArray();
            七対子向聴数 = hand.七対子向聴数;
            国士無双向聴数 = hand.国士無双向聴数;

            redTileCount = hand.AllTiles.Count(_ => _.red);
        }
        public readonly int 向聴数
        {
            get
            {
                // あがりに必要なのは4面子1雀頭。
                // http://ara.moo.jp/mjhmr/shanten.htm
                var result = 8;

                result -= melds.Length * 2;
                result -= Sets.Length * 2;

                var lackSetsCount = 4 - melds.Length - Sets.Length;
                var use塔子Count = Mathf.Min(塔子.Length, lackSetsCount);
                result -= use塔子Count;
                lackSetsCount -= use塔子Count;

                var usePairCount = Mathf.Min(Pairs.Length, lackSetsCount);
                result -= usePairCount;

                if (Pairs.Length > usePairCount)
                {
                    result -= 1;
                }

                if (melds.Length == 0)
                {
                    result = Mathf.Min(result, 七対子向聴数);
                    result = Mathf.Min(result, 国士無双向聴数);
                }

                return result;
            }
        }
        public static (int 向聴数, Structure[]) Build(Hand hand)
        {
            var 向聴数 = int.MaxValue;
            var structures = new List<Structure>();

            foreach (var it in CollectStructures(hand))
            {
                var score = it.向聴数;
                if (向聴数 > score)
                {
                    向聴数 = score;
                    structures.Clear();
                    structures.Add(it);
                }
                else if (向聴数 == score)
                {
                    structures.Add(it);
                }
            }

            return (向聴数, structures.ToArray());
        }

        public static bool 向聴数IsLessThanOrEqual(Hand hand, int value)
        {
            if (hand.七対子向聴数 <= value)
            {
                return true;
            }
            if (hand.国士無双向聴数 <= value)
            {
                return true;
            }
            foreach (var it in CollectStructures(hand))
            {
                if (it.向聴数 <= value)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool 向聴数IsLessThan(Hand hand, int value)
        {
            return 向聴数IsLessThanOrEqual(hand, value - 1);
        }

        static IEnumerable<Structure> CollectStructures(Hand hand)
        {
            Debug.Assert(hand.tiles.Count % 3 != 0, "wrong hand tile count : " + hand.tiles.Count.ToString());

            // queueよりstackのほうが速い。足切り向聴数が早く出てくるからだ。
            var tasks = new Stack<Structure>();
            tasks.Push(new Structure(hand));

            while (tasks.Count > 0)
            {
                var task = tasks.Pop();

                if (task.unsolvedTiles.Length == 0)
                {
                    yield return task;
                    continue;
                }

                var tile = task.unsolvedTiles[0];
                if (Array.IndexOf(task.IsolatedTiles, tile) >= 0)
                {
                    continue;
                }
                // 浮き牌
                {
                    // ただし浮き牌内で塔子、対子ができないようにする
                    bool canAddIsolatedTile = true;
                    if (tile.IsSuited())
                    {
                        if (tile.Number() > 1)
                        {
                            var minusOne = TileTypeUtil.Get(tile.Suit(), tile.Number() - 1);
                            if (System.Array.IndexOf(task.IsolatedTiles, minusOne) >= 0)
                            {
                                canAddIsolatedTile = false;
                            }
                        }
                        if (tile.Number() > 2)
                        {
                            var minusTwo = TileTypeUtil.Get(tile.Suit(), tile.Number() - 2);
                            if (System.Array.IndexOf(task.IsolatedTiles, minusTwo) >= 0)
                            {
                                canAddIsolatedTile = false;
                            }
                        }
                    }
                    if (canAddIsolatedTile)
                    {
                        var structure = new Structure(task,
                            unsolvedTiles: task.unsolvedTiles.AsSpan(1).ToArray(),
                            isolatedTiles: Append(task.IsolatedTiles, tile));
                        tasks.Push(structure);
                    }
                }

                if (tile.IsSuited() && tile.Number() <= 8)
                {
                    var plusOne = TileTypeUtil.Get(tile.Suit(), tile.Number() + 1);
                    // 塔子
                    if (Array.IndexOf(task.unsolvedTiles, plusOne) >= 0)
                    {
                        var structure = new Structure(task,
                            unsolvedTiles: Remove(task.unsolvedTiles, tile, plusOne).ToArray(),
                            塔子: Append(task.塔子, (tile, plusOne)));
                        tasks.Push(structure);
                    }

                    if (tile.Number() <= 7)
                    {
                        var plusTwo = TileTypeUtil.Get(tile.Suit(), tile.Number() + 2);

                        // 塔子
                        if (Array.IndexOf(task.unsolvedTiles, plusTwo) >= 0)
                        {
                            {
                                var structure = new Structure(task,
                                    unsolvedTiles: Remove(task.unsolvedTiles, tile, plusTwo).ToArray(),
                                    塔子: Append(task.塔子, (tile, plusTwo)));
                                tasks.Push(structure);
                            }

                            // 順子
                            if (Array.IndexOf(task.unsolvedTiles, plusOne) >= 0)
                            {
                                var structure = new Structure(task,
                                    unsolvedTiles: Remove(task.unsolvedTiles, tile, plusOne, plusTwo).ToArray(),
                                    sets: Append(task.Sets, new Set(tile, plusOne, plusTwo)));
                                tasks.Push(structure);
                            }
                        }
                    }
                }

                // 対子
                if (task.unsolvedTiles.Length >= 2 && task.unsolvedTiles[1] == tile)
                {
                    // 同じ対子が二組あるのは許可しない。
                    if (System.Array.IndexOf(task.Pairs, tile) == -1)
                    {
                        var structure = new Structure(task,
                            unsolvedTiles: task.unsolvedTiles.AsSpan(2).ToArray(),
                            pairs: Append(task.Pairs, tile));
                        tasks.Push(structure);
                    }

                    // 刻子
                    if (task.unsolvedTiles.Length >= 3 && task.unsolvedTiles[2] == tile)
                    {
                        var structure = new Structure(task,
                            unsolvedTiles: task.unsolvedTiles.AsSpan(3).ToArray(),
                            sets: Append(task.Sets, new Set(tile, tile, tile)));
                        tasks.Push(structure);
                    }
                }
            }
        }

        public readonly IEnumerable<TileType> AllUsedTiles
        {
            get
            {
                foreach (var it in unsolvedTiles)
                {
                    yield return it;
                }
                foreach (var it in IsolatedTiles)
                {
                    yield return it;
                }
                foreach (var set in Sets)
                {
                    yield return set.first;
                    yield return set.second;
                    yield return set.third;
                }
                foreach (var it in Pairs)
                {
                    yield return it;
                    yield return it;
                }
                foreach (var (left, right) in 塔子)
                {
                    yield return left;
                    yield return right;
                }
                foreach (var meld in melds)
                {
                    foreach (var (tile, _) in meld.tileFroms)
                    {
                        yield return tile.type;
                    }
                }
            }
        }

        static T[] Append<T>(T[] array, T item)
        {
            var result = new T[array.Length + 1];
            array.CopyTo(result, 0);
            result[^1] = item;
            return result;
        }

        static Span<TileType> Remove(TileType[] array, TileType item1, TileType item2, TileType? item3 = null)
        {
            var copy = new TileType[array.Length].AsSpan();
            array.CopyTo(copy);

            copy = Remove(copy, item1);
            copy = Remove(copy, item2);

            if (item3.HasValue)
            {
                copy = Remove(copy, item3.Value);
            }

            return copy;

            static Span<TileType> Remove(Span<TileType> span, TileType item)
            {
                for (int i = 0; i < span.Length; ++i)
                {
                    if (span[i] == item)
                    {
                        for (int j = i; j < span.Length - 1; ++j)
                        {
                            span[j] = span[j + 1];
                        }
                        return span[..^1];
                    }
                }
                return span;
            }
        }
    }
}
