#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSKT.Mahjongs.Rounds;
using TSKT.Mahjongs.Hands;

namespace TSKT.Mahjongs
{
    public class 手牌
    {
        readonly Player owner;
        readonly public List<Tile> tiles = new();
        readonly public List<副露> 副露 = new();

        public 手牌(Player owner)
        {
            this.owner = owner;
        }

        手牌(in Serializables.Hand source, Player owner)
        {
            this.owner = owner;
            副露 = source.melds.Select(_ => _.Deserialize(owner.局.壁牌)).ToList();
            tiles = source.tiles.Select(_ => owner.局.壁牌.allTiles[_]).ToList();
        }

        static public 手牌 FromSerializable(in Serializables.Hand source, Player owner)
        {
            return new 手牌(source, owner);
        }
        public Serializables.Hand ToSerializable()
        {
            return new Serializables.Hand(this);
        }

        public void Sort()
        {
            if (Discarding)
            {
                tiles.Sort(0, tiles.Count - 1, null);
            }
            else
            {
                tiles.Sort();
            }
        }

        public 手牌 Clone()
        {
            var result = new 手牌(owner);
            result.tiles.AddRange(tiles);
            result.副露.AddRange(副露);
            return result;
        }

        public Hands.Solution Solve()
        {
            return new Hands.Solution(this);
        }

        public bool 向聴数IsLessThanOrEqual(int value)
        {
            return Hands.Structure.向聴数IsLessThanOrEqual(this, value);
        }

        public bool 向聴数IsLessThan(int value)
        {
            return Hands.Structure.向聴数IsLessThan(this, value);
        }

        public bool 九種九牌 => tiles.Where(_ => _.type.么九牌()).Distinct().Count() >= 9;

        public IEnumerable<Tile> AllTiles
        {
            get
            {
                foreach (var it in tiles)
                {
                    yield return it;
                }
                foreach (var meld in 副露)
                {
                    foreach (var (tile, from) in meld.tileFroms)
                    {
                        yield return tile;
                    }
                }
            }
        }

        public void BuildClosedQuad(TileType tileType)
        {
            var quadTiles = new (Tile, PlayerIndex)[4];
            for (int i = 0; i < 4; ++i)
            {
                var tile = tiles.First(_ => _.type == tileType);
                tiles.Remove(tile);
                quadTiles[i] = (tile, owner.index);
            }
            var meld = new 副露(quadTiles);
            副露.Add(meld);
        }
        
        public TileType[] Get和了牌()
        {
            if (!向聴数IsLessThanOrEqual(0))
            {
                return System.Array.Empty<TileType>();
            }
            return Get有効牌();
        }

        public TileType[] Get有効牌()
        {
            var currentScore = new Solution(this).向聴数;
            var tilesToCheck = new List<TileType>();

            // 待ち牌候補
            // 　一九字牌 : 国士無双
            // 　手牌と同じ牌：対子、暗刻
            // 　手牌の隣牌、二つ隣の牌：順子
            foreach (var it in tiles)
            {
                tilesToCheck.Add(it.type);

                if (it.type.Is数牌())
                {
                    var number = it.type.Number();
                    if (number > 1)
                    {
                        tilesToCheck.Add(TileTypeUtil.Get(it.type.Suit(), number - 1));
                    }
                    if (number > 2)
                    {
                        tilesToCheck.Add(TileTypeUtil.Get(it.type.Suit(), number - 2));
                    }
                    if (number < 9)
                    {
                        tilesToCheck.Add(TileTypeUtil.Get(it.type.Suit(), number + 1));
                    }
                    if (number < 8)
                    {
                        tilesToCheck.Add(TileTypeUtil.Get(it.type.Suit(), number + 2));
                    }
                }
            }
            foreach (TileType tile in System.Enum.GetValues(typeof(TileType)))
            {
                if (tile.么九牌())
                {
                    tilesToCheck.Add(tile);
                }
            }

            var allTilesInHand = AllTiles.ToArray();
            var result = new List<TileType>();
            foreach (var tile in tilesToCheck.Distinct())
            {
                // 手牌内で4枚使っている場合は待ち牌扱いにしない
                // e.g. P1が4枚あるときにP1のシャンポンや単騎にはならない
                if (allTilesInHand.Count(_ => _.type == tile) == 4)
                {
                    continue;
                }

                var clone = Clone();
                clone.tiles.Add(new Tile(0, tile, false));
                if (clone.向聴数IsLessThan(currentScore))
                {
                    result.Add(tile);
                }
            }

            return result.ToArray();
        }

        public bool Discarding => tiles.Count % 3 == 2;

        public int 国士無双向聴数
        {
            get
            {
                if (副露.Count > 0)
                {
                    return int.MaxValue;
                }
                var tileCounts = tiles
                    .Select(_ => _.type)
                    .Where(_ => _.么九牌())
                    .GroupBy(_ => _)
                    .Select(_ => _.Count())
                    .ToArray();
                var result = 13 - tileCounts.Length;

                if (tileCounts.Any(_ => _ >= 2))
                {
                    result -= 1;
                }
                return result;
            }
        }
        public int 七対子向聴数
        {
            get
            {
                if (副露.Count > 0)
                {
                    return int.MaxValue;
                }

                var pairCount = tiles.Select(_ => _.type).GroupBy(_ => _)
                    .Select(_ => _.Count())
                    .Where(_ => _ >= 2)
                    .Count();

                var result = 6 - pairCount;
                var typeCount = tiles.Select(_ => _.type).Distinct().Count();
                if (typeCount < 7)
                {
                    result += 7 - typeCount;
                }
                return result;
            }
        }
    }

    /// <summary>
    /// 副露
    /// </summary>
    public readonly struct 副露
    {
        public readonly (Tile tile, PlayerIndex fromPlayerIndex)[] tileFroms;

        public readonly bool 順子 => tileFroms[0].tile.type != tileFroms[1].tile.type;
        public readonly bool 槓子 => tileFroms.Length == 4;
        public readonly bool 暗槓
        {
            get
            {
                if (!槓子)
                {
                    return false;
                }
                foreach (var (_, fromPlayerIndex) in tileFroms)
                {
                    if (fromPlayerIndex != tileFroms[0].fromPlayerIndex)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public readonly Tile Min => tileFroms[0].tile;

        public 副露(params (Tile tile, PlayerIndex fromPlayerIndex)[] tileFroms)
        {
            this.tileFroms = tileFroms.OrderBy(_ => _.tile.type).ToArray();
        }
        
        副露(in Serializables.Meld source, 壁牌 wallTile)
        {
            tileFroms = source.tileFroms
                .Select(_ => (wallTile.allTiles[_.tile], _.fromPlayerIndex))
                .ToArray();
        }

        static public 副露 FromSerializable(in Serializables.Meld source, 壁牌 wallTile)
        {
            return new 副露(source, wallTile);
        }

        public readonly Serializables.Meld ToSerializable()
        {
            return new Serializables.Meld(this);
        }

        public readonly bool TryGetTileFromOtherPlayer(out (Tile tile, PlayerIndex fromPlayerIndex) result)
        {
            PlayerIndex ownerPlayerIndex;
            if (tileFroms[0].fromPlayerIndex == tileFroms[1].fromPlayerIndex)
            {
                ownerPlayerIndex = tileFroms[0].fromPlayerIndex;
            }
            else if (tileFroms[0].fromPlayerIndex == tileFroms[2].fromPlayerIndex)
            {
                ownerPlayerIndex = tileFroms[0].fromPlayerIndex;
            }
            else
            {
                ownerPlayerIndex = tileFroms[1].fromPlayerIndex;
            }

            foreach (var it in tileFroms)
            {
                if (it.fromPlayerIndex != ownerPlayerIndex)
                {
                    result = it;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public readonly bool Is喰い替え(Tile tileToDiscard)
        {
            if (!TryGetTileFromOtherPlayer(out var tileFromOtherPlayer))
            {
                return false;
            }
            if (tileToDiscard.type == tileFromOtherPlayer.tile.type)
            {
                return true;
            }

            if (順子)
            {
                if (tileToDiscard.type.Is数牌())
                {
                    if (tileFroms[0].tile.type.Suit() == tileToDiscard.type.Suit())
                    {
                        var tilesNumbers = tileFroms
                            .Where(_ => _.fromPlayerIndex != tileFromOtherPlayer.fromPlayerIndex)
                            .Select(_ => _.tile.type.Number())
                            .Append(tileToDiscard.type.Number());
                        if (tilesNumbers.Sum() % 3 == 0)
                        {
                            if (tilesNumbers.Max() - tilesNumbers.Min() == 2)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
    public static class 副露Util
    {
        public static (TileType, bool, TileType, bool, TileType, bool) GetKey(Tile a, Tile b, Tile c)
        {
            var k = new[] { a, b, c };
            System.Array.Sort(k, static (left, right) =>
            {
                if (left.type > right.type)
                {
                    return 1;
                }
                if (left.type < right.type)
                {
                    return -1;
                }
                if (left.red && !right.red)
                {
                    return 1;
                }
                if (!left.red && right.red)
                {
                    return -1;
                }
                return 0;
            });
            return (k[0].type, k[0].red, k[1].type, k[1].red, k[2].type, k[2].red);
        }

    }
}
