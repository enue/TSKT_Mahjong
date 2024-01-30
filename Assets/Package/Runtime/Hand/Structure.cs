using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable

namespace TSKT.Mahjongs.Hands
{
    public readonly struct Structure
    {
        public readonly struct 面子
        {
            public readonly TileType first;
            public readonly TileType second;
            public readonly TileType third;

            public 面子(TileType first, TileType second, TileType third)
            {
                this.first = first;
                this.second = second;
                this.third = third;
            }

            readonly public bool 刻子 => first == second;
            readonly public bool 順子 => !刻子;
        }

        readonly TileType[] unsolvedTiles;
        public TileType[] 浮き牌 { get; }
        public 面子[] 面子s { get; }
        public TileType[] 対子 { get; }
        public (TileType left, TileType right)[] 塔子 { get; }
        public readonly 副露[] 副露;
        readonly int 国士無双向聴数;
        readonly int 七対子向聴数;
        public readonly int 赤牌;

        Structure(Structure source, TileType[] unsolvedTiles, TileType[]? 浮き牌 = null, 面子[]? sets = null, TileType[]? pairs = null, (TileType left, TileType right)[]? 塔子 = null)
        {
            this.unsolvedTiles = unsolvedTiles;
            this.浮き牌 = 浮き牌 ?? source.浮き牌;
            面子s = sets ?? source.面子s;
            対子 = pairs ?? source.対子;
            this.塔子 = 塔子 ?? source.塔子;

            副露 = source.副露;
            国士無双向聴数 = source.国士無双向聴数;
            七対子向聴数 = source.七対子向聴数;
            赤牌 = source.赤牌;
        }
        public Structure(手牌 hand)
        {
            浮き牌 = System.Array.Empty<TileType>();
            面子s = System.Array.Empty<面子>();
            対子 = System.Array.Empty<TileType>();
            塔子 = System.Array.Empty<(TileType, TileType)>();

            unsolvedTiles = hand.tiles.Select(_ => _.type).ToArray();
            System.Array.Sort(unsolvedTiles);
            副露 = hand.副露.ToArray();
            七対子向聴数 = hand.七対子向聴数;
            国士無双向聴数 = hand.国士無双向聴数;

            赤牌 = hand.AllTiles.Count(_ => _.red);
        }
        public readonly int 向聴数
        {
            get
            {
                // あがりに必要なのは4面子1雀頭。
                // http://ara.moo.jp/mjhmr/shanten.htm
                var result = 8;

                result -= 副露.Length * 2;
                result -= 面子s.Length * 2;

                var lackSetsCount = 4 - 副露.Length - 面子s.Length;
                var use塔子Count = Mathf.Min(塔子.Length, lackSetsCount);
                result -= use塔子Count;
                lackSetsCount -= use塔子Count;

                var usePairCount = Mathf.Min(対子.Length, lackSetsCount);
                result -= usePairCount;

                if (対子.Length > usePairCount)
                {
                    result -= 1;
                }

                if (副露.Length == 0 && 塔子.Length == 0 && 面子s.Length == 0)
                {
                    result = Mathf.Min(result, 七対子向聴数);
                    result = Mathf.Min(result, 国士無双向聴数);
                }

                return result;
            }
        }
        public static (int 向聴数, Structure[]) Build(手牌 hand)
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

        public static bool 向聴数IsLessThanOrEqual(手牌 hand, int value)
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

        public static bool 向聴数IsLessThan(手牌 hand, int value)
        {
            return 向聴数IsLessThanOrEqual(hand, value - 1);
        }

        static IEnumerable<Structure> CollectStructures(手牌 hand)
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
                if (Array.IndexOf(task.浮き牌, tile) >= 0)
                {
                    continue;
                }
                // 浮き牌に追加
                // ただし浮き牌内で対子はできないようになっている
                {
                    var structure = new Structure(task,
                        unsolvedTiles: task.unsolvedTiles.AsSpan(1).ToArray(),
                        浮き牌: Append(task.浮き牌, tile));
                    tasks.Push(structure);
                }

                if (tile.Is数牌() && tile.Number() <= 8)
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
                                    sets: Append(task.面子s, new 面子(tile, plusOne, plusTwo)));
                                tasks.Push(structure);
                            }
                        }
                    }
                }

                // 対子
                if (task.unsolvedTiles.Length >= 2 && task.unsolvedTiles[1] == tile)
                {
                    // 同じ対子が二組あるのは許可しない。
                    if (System.Array.IndexOf(task.対子, tile) == -1)
                    {
                        var structure = new Structure(task,
                            unsolvedTiles: task.unsolvedTiles.AsSpan(2).ToArray(),
                            pairs: Append(task.対子, tile));
                        tasks.Push(structure);
                    }

                    // 刻子
                    if (task.unsolvedTiles.Length >= 3 && task.unsolvedTiles[2] == tile)
                    {
                        var structure = new Structure(task,
                            unsolvedTiles: task.unsolvedTiles.AsSpan(3).ToArray(),
                            sets: Append(task.面子s, new 面子(tile, tile, tile)));
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
                foreach (var it in 浮き牌)
                {
                    yield return it;
                }
                foreach (var set in 面子s)
                {
                    yield return set.first;
                    yield return set.second;
                    yield return set.third;
                }
                foreach (var it in 対子)
                {
                    yield return it;
                    yield return it;
                }
                foreach (var (left, right) in 塔子)
                {
                    yield return left;
                    yield return right;
                }
                foreach (var meld in 副露)
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
