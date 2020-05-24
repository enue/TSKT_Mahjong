using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TSKT.Mahjongs.Hands
{
    public class Structure
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

            public bool 刻子 => first == second;
            public bool 順子 => !刻子;
        }

        readonly List<TileType> unsolvedTiles;
        public TileType[] IsolatedTiles { get; private set; } = System.Array.Empty<TileType>();
        public Set[] Sets { get; private set; } = System.Array.Empty<Set>();
        public TileType[] Pairs { get; private set; } = System.Array.Empty<TileType>();
        public (TileType left, TileType right)[] 塔子 { get; private set; } = System.Array.Empty<(TileType, TileType)>();
        public readonly Meld[] melds;
        public readonly int 国士無双向聴数;
        public readonly int redTileCount;

        public Structure(Structure source)
        {
            unsolvedTiles = source.unsolvedTiles.ToList();
            IsolatedTiles = source.IsolatedTiles;
            Sets = source.Sets;
            Pairs = source.Pairs;
            塔子 = source.塔子;
            melds = source.melds;
            国士無双向聴数 = source.国士無双向聴数;
            redTileCount = source.redTileCount;
        }

        public Structure(Hand hand)
        {
            unsolvedTiles = hand.tiles.Select(_ => _.type).ToList();
            unsolvedTiles.Sort();
            melds = hand.melds.ToArray();

            if (melds.Length > 0)
            {
                国士無双向聴数 = int.MaxValue;
            }
            else
            {
                var count = new IntDictionary<TileType>();
                foreach (var it in unsolvedTiles)
                {
                    if (it.么九牌())
                    {
                        ++count[it];
                    }
                }
                国士無双向聴数 = 13 - count.Count;
                if (count.Any(_ => _.Value >= 2))
                {
                    国士無双向聴数 -= 1;
                }
            }
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
                    // 七対子の場合
                    result = Mathf.Min(result, 6 - Pairs.Length);
                }
                result = Mathf.Min(result, 国士無双向聴数);

                return result;
            }
        }

        int LackCount
        {
            get
            {
                if (Pairs.Length > 0)
                {
                    return Pairs.Length - 1 + 塔子.Length + IsolatedTiles.Length * 2;
                }
                if (IsolatedTiles.Length > 0)
                {
                    return 塔子.Length + IsolatedTiles.Length * 2 - 1;
                }
                return 塔子.Length;
            }
        }

        static public (int 向聴数, List<Structure>) Build(Hand hand)
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

            return (向聴数, structures);
        }

        static public bool 向聴数IsLessThanOrEqual(Hand hand, int value)
        {
            foreach(var it in CollectStructures(hand, allowLackCount: value + 1))
            {
                if (it.向聴数 <= value)
                {
                    return true;
                }
            }
            return false;
        }

        static IEnumerable<Structure> CollectStructures(Hand hand, int allowLackCount = int.MaxValue)
        {
            // queueよりstackのほうが速い。足切り向聴数が早く出てくるからだ。
            var tasks = new Stack<Structure>();
            tasks.Push(new Structure(hand));

            while (tasks.Count > 0)
            {
                var task = tasks.Pop();

                if (task.unsolvedTiles.Count == 0)
                {
                    yield return task;

                    allowLackCount = Mathf.Min(allowLackCount, task.向聴数 + 1);
                    continue;
                }

                var tile = task.unsolvedTiles[0];
                if (task.LackCount <= allowLackCount)
                {
                    // 刻子
                    if (task.unsolvedTiles.Count(_ => _ == tile) >= 3)
                    {
                        var structure = new Structure(task);
                        for (int i = 0; i < 3; ++i)
                        {
                            structure.unsolvedTiles.Remove(tile);
                        }
                        var sets = structure.Sets.ToList();
                        sets.Add(new Set(tile, tile, tile));
                        structure.Sets = sets.ToArray();
                        tasks.Push(structure);
                    }

                    if (tile.IsSuited()
                        && tile.Number() <= 8)
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
                                structure.unsolvedTiles.Remove(tile);
                                structure.unsolvedTiles.Remove(plusOne);
                                structure.unsolvedTiles.Remove(plusTwo);
                                var sets = structure.Sets.ToList();
                                sets.Add(new Set(tile, plusOne, plusTwo));
                                structure.Sets = sets.ToArray();
                                tasks.Push(structure);
                            }
                            // 塔子
                            if (task.LackCount < allowLackCount)
                            {
                                if (task.unsolvedTiles.Contains(plusTwo))
                                {
                                    var structure = new Structure(task);
                                    structure.unsolvedTiles.Remove(tile);
                                    structure.unsolvedTiles.Remove(plusTwo);
                                    var tou = structure.塔子.ToList();
                                    tou.Add((tile, plusTwo));
                                    structure.塔子 = tou.ToArray();
                                    tasks.Push(structure);
                                }
                            }
                        }
                        // 塔子
                        if (task.LackCount < allowLackCount)
                        {
                            if (task.unsolvedTiles.Contains(plusOne))
                            {
                                var structure = new Structure(task);
                                structure.unsolvedTiles.Remove(tile);
                                structure.unsolvedTiles.Remove(plusOne);
                                var tou = structure.塔子.ToList();
                                tou.Add((tile, plusOne));
                                structure.塔子 = tou.ToArray();
                                tasks.Push(structure);
                            }
                        }
                    }
                }
                // 頭
                // 基本的に二つ（シャンポン）まで。七対子の目がある場合は制限なし。
                if ((task.melds.Length == 0 && task.Sets.Length == 0 && task.塔子.Length == 0)
                    || (task.Pairs.Length < 2)
                    || (task.LackCount < allowLackCount))
                {
                    if (task.unsolvedTiles.Count(_ => _ == tile) >= 2)
                    {
                        // 同じ対子が二組あるのは許可しない。
                        if (System.Array.IndexOf(task.Pairs, tile) == -1)
                        {
                            var structure = new Structure(task);
                            for (int i = 0; i < 2; ++i)
                            {
                                structure.unsolvedTiles.Remove(tile);
                            }
                            var pairs = structure.Pairs.ToList();
                            pairs.Add(tile);
                            structure.Pairs = pairs.ToArray();
                            tasks.Push(structure);
                        }
                    }
                }
                // 浮き牌
                // 基本的に一つ（単騎）まで。国士無双の目がある場合は制限なし。
                if ((task.melds.Length == 0 && task.Sets.Length <= 1 && task.塔子.Length == 0)
                    || (task.Pairs.Length <= 1)
                    || (task.LackCount < allowLackCount))
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
                        structure.unsolvedTiles.Remove(tile);
                        var isolatedTiles = structure.IsolatedTiles.ToList();
                        isolatedTiles.Add(tile);
                        structure.IsolatedTiles = isolatedTiles.ToArray();
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

    }
}