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
        public readonly struct Meld
        {
            public readonly TileType[] sortedTiles;
            public bool 暗槓 { get; }
            public bool 槓子 => sortedTiles.Length == 4;
            public bool 順子 => sortedTiles[0] != sortedTiles[1];

            public Meld(Mahjongs.Meld source)
            {
                sortedTiles = source.Sorted.Select(_ => _.type).ToArray();
                暗槓 = source.tileFroms.Select(_ => _.fromPlayerIndex).Distinct().Count() == 1;
            }
        }

        public readonly List<TileType> unsolvedTiles;
        public readonly List<TileType> isolatedTiles = new List<TileType>();
        public readonly List<Set> sets = new List<Set>();
        public readonly List<TileType> pairs = new List<TileType>();
        public readonly List<(TileType left, TileType right)> 塔子 = new List<(TileType, TileType)>();
        public readonly Meld[] melds;
        public readonly int 国士無双向聴数;
        public readonly int redTileCount;

        public Structure(Structure source)
        {
            unsolvedTiles = source.unsolvedTiles.ToList();
            isolatedTiles.AddRange(source.isolatedTiles);
            sets.AddRange(source.sets);
            pairs.AddRange(source.pairs);
            塔子.AddRange(source.塔子);
            melds = source.melds;
            国士無双向聴数 = source.国士無双向聴数;
            redTileCount = source.redTileCount;
        }

        public Structure(Hand hand)
        {
            unsolvedTiles = hand.tiles.Select(_ => _.type).ToList();
            unsolvedTiles.Sort();
            melds = hand.melds.Select(_ => new Meld(_)).ToArray();

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
                result -= sets.Count * 2;

                var lackSetsCount = 4 - melds.Length - sets.Count;
                var use塔子Count = Mathf.Min(塔子.Count, lackSetsCount);
                result -= use塔子Count;
                lackSetsCount -= use塔子Count;

                var usePairCount = Mathf.Min(pairs.Count, lackSetsCount);
                result -= usePairCount;

                if (pairs.Count > usePairCount)
                {
                    result -= 1;
                }

                if (melds.Length == 0)
                {
                    // 七対子の場合
                    result = Mathf.Min(result, 6 - pairs.Count);
                }
                result = Mathf.Min(result, 国士無双向聴数);

                return result;
            }
        }

        static public (int 向聴数, List<Structure>) Build(Hand hand)
        {
            var result = (向聴数: int.MaxValue, structures: new List<Structure>());
            var queue = new Queue<Structure>();
            queue.Enqueue(new Structure(hand));

            while (queue.Count > 0)
            {
                var task = queue.Dequeue();

                if (task.unsolvedTiles.Count == 0)
                {
                    var score = task.向聴数;
                    if (result.向聴数 > score)
                    {
                        result.向聴数 = score;
                        result.structures.Clear();
                        result.structures.Add(task);
                    }
                    else if (result.向聴数 == score)
                    {
                        result.向聴数 = score;
                        result.structures.Add(task);
                    }
                    continue;
                }

                var tile = task.unsolvedTiles[0];
                // 刻子
                if (task.unsolvedTiles.Count(_ => _ == tile) >= 3)
                {
                    var structure = new Structure(task);
                    for (int i = 0; i < 3; ++i)
                    {
                        structure.unsolvedTiles.Remove(tile);
                    }
                    structure.sets.Add(new Set(tile, tile, tile));
                    queue.Enqueue(structure);
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
                            structure.sets.Add(new Set(tile, plusOne, plusTwo));
                            queue.Enqueue(structure);
                        }
                        // 塔子
                        if (task.unsolvedTiles.Contains(plusTwo))
                        {
                            var structure = new Structure(task);
                            structure.unsolvedTiles.Remove(tile);
                            structure.unsolvedTiles.Remove(plusTwo);
                            structure.塔子.Add((tile, plusTwo));
                            queue.Enqueue(structure);
                        }
                    }
                    // 塔子
                    if (task.unsolvedTiles.Contains(plusOne))
                    {
                        var structure = new Structure(task);
                        structure.unsolvedTiles.Remove(tile);
                        structure.unsolvedTiles.Remove(plusOne);
                        structure.塔子.Add((tile, plusOne));
                        queue.Enqueue(structure);
                    }
                }
                // 頭
                if (task.unsolvedTiles.Count(_ => _ == tile) >= 2)
                {
                    // 同じ対子が二組あるのは許可しない。
                    if (!task.pairs.Contains(tile))
                    {
                        var structure = new Structure(task);
                        for (int i = 0; i < 2; ++i)
                        {
                            structure.unsolvedTiles.Remove(tile);
                        }
                        structure.pairs.Add(tile);
                        queue.Enqueue(structure);
                    }
                }
                // 浮き牌
                // ただし浮き牌内で塔子、対子ができないようにする
                {
                    bool canAddIsolatedTile = true;
                    if (task.isolatedTiles.Contains(tile))
                    {
                        canAddIsolatedTile = false;
                    }
                    if (tile.IsSuited())
                    {
                        if (tile.Number() > 1)
                        {
                            var minusOne = TileTypeUtil.Get(tile.Suit(), tile.Number() - 1);
                            if (task.isolatedTiles.Contains(minusOne))
                            {
                                canAddIsolatedTile = false;
                            }
                        }
                        if (tile.Number() > 2)
                        {
                            var minusTwo = TileTypeUtil.Get(tile.Suit(), tile.Number() - 2);
                            if (task.isolatedTiles.Contains(minusTwo))
                            {
                                canAddIsolatedTile = false;
                            }
                        }
                    }
                    if (canAddIsolatedTile)
                    {
                        var structure = new Structure(task);
                        structure.unsolvedTiles.Remove(tile);
                        structure.isolatedTiles.Add(tile);
                        queue.Enqueue(structure);
                    }
                }
            }

            return result;
        }

        public IEnumerable<TileType> AllUsedTiles
        {
            get
            {
                foreach (var it in unsolvedTiles)
                {
                    yield return it;
                }
                foreach (var it in isolatedTiles)
                {
                    yield return it;
                }
                foreach (var set in sets)
                {
                    yield return set.first;
                    yield return set.second;
                    yield return set.third;
                }
                foreach (var it in pairs)
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
                    foreach (var tile in meld.sortedTiles)
                    {
                        yield return tile;
                    }
                }
            }
        }

    }
}