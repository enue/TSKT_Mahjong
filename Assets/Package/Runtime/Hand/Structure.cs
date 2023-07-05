using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable

namespace TSKT.Mahjongs.Hands
{
    public struct Structure
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

        TileType[] unsolvedTiles;
        public TileType[] IsolatedTiles { get; private set; }
        public Set[] Sets { get; private set; }
        public TileType[] Pairs { get; private set; }
        public (TileType left, TileType right)[] 塔子 { get; private set; }
        public readonly Meld[] melds;
        public readonly int 国士無双向聴数;
        public readonly int 七対子向聴数;
        public readonly int redTileCount;

        public Structure(Structure source)
        {
            unsolvedTiles = source.unsolvedTiles;
            IsolatedTiles = source.IsolatedTiles;
            Sets = source.Sets;
            Pairs = source.Pairs;
            塔子 = source.塔子;
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
        public int 向聴数
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
        static public (int 向聴数, Structure[]) Build(Hand hand)
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

        static public bool 向聴数IsLessThanOrEqual(Hand hand, int value)
        {
            foreach(var it in CollectStructures(hand))
            {
                if (it.向聴数 <= value)
                {
                    return true;
                }
            }
            return false;
        }

        static public bool 向聴数IsLessThan(Hand hand, int value)
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
                if (task.IsolatedTiles.Contains(tile))
                {
                    continue;
                }
                // 対子
                if (task.unsolvedTiles.Length >= 2 && task.unsolvedTiles[1] == tile)
                {
                    // 同じ対子が二組あるのは許可しない。
                    if (System.Array.IndexOf(task.Pairs, tile) == -1)
                    {
                        var structure = new Structure(task);
                        structure.unsolvedTiles = structure.unsolvedTiles.AsSpan(2).ToArray();
                        structure.Pairs = Append(structure.Pairs, tile);
                        tasks.Push(structure);
                    }

                    // 刻子
                    if (task.unsolvedTiles.Length >= 3 && task.unsolvedTiles[2] == tile)
                    {
                        var structure = new Structure(task);
                        structure.unsolvedTiles = structure.unsolvedTiles.AsSpan(3).ToArray();
                        structure.Sets = Append(structure.Sets, new Set(tile, tile, tile));
                        tasks.Push(structure);
                    }
                }

                if (tile.IsSuited() && tile.Number() <= 8)
                {
                    var plusOne = TileTypeUtil.Get(tile.Suit(), tile.Number() + 1);
                    if (tile.Number() <= 7)
                    {
                        var plusTwo = TileTypeUtil.Get(tile.Suit(), tile.Number() + 2);

                        // 順子
                        if (task.unsolvedTiles.Contains(plusOne)
                            && task.unsolvedTiles.Contains(plusTwo))
                        {
                            var structure = new Structure(task);
                            var unsolvedTiles = structure.unsolvedTiles.ToList();
                            unsolvedTiles.Remove(tile);
                            unsolvedTiles.Remove(plusOne);
                            unsolvedTiles.Remove(plusTwo);
                            structure.unsolvedTiles = unsolvedTiles.ToArray();

                            structure.Sets = Append(structure.Sets, new Set(tile, plusOne, plusTwo));
                            tasks.Push(structure);
                        }
                        // 塔子
                        if (task.unsolvedTiles.Contains(plusTwo))
                        {
                            var structure = new Structure(task);
                            var unsolvedTiles = structure.unsolvedTiles.ToList();
                            unsolvedTiles.Remove(tile);
                            unsolvedTiles.Remove(plusTwo);
                            structure.unsolvedTiles = unsolvedTiles.ToArray();

                            structure.塔子 = Append(structure.塔子, (tile, plusTwo));
                            tasks.Push(structure);
                        }
                    }
                    // 塔子
                    if (task.unsolvedTiles.Contains(plusOne))
                    {
                        var structure = new Structure(task);
                        var unsolvedTiles = structure.unsolvedTiles.ToList();
                        unsolvedTiles.Remove(tile);
                        unsolvedTiles.Remove(plusOne);
                        structure.unsolvedTiles = unsolvedTiles.ToArray();

                        structure.塔子 = Append(structure.塔子, (tile, plusOne));
                        tasks.Push(structure);
                    }
                }
                // 浮き牌
                {
                    // ただし浮き牌内で塔子、対子ができないようにする
                    bool canAddIsolatedTile = true;
                    if (System.Array.IndexOf(task.IsolatedTiles, tile) >= 0)
                    {
                        canAddIsolatedTile = false;
                    }
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
                        var structure = new Structure(task);
                        structure.unsolvedTiles = structure.unsolvedTiles.AsSpan(1).ToArray();
                        structure.IsolatedTiles = Append(structure.IsolatedTiles, tile);
                        tasks.Push(structure);
                    }
                }
            }
        }

        public IEnumerable<TileType> AllUsedTiles
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
    }
}