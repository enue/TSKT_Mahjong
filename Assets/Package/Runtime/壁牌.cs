using System.Collections;
using System.Collections.Generic;
using System.Linq;
#nullable enable

namespace TSKT.Mahjongs
{
    public class 壁牌
    {
        readonly public uint randomSeed;
        readonly public List<Tile> tiles = new();
        readonly public Tile[] allTiles;

        public 壁牌(Rules.赤牌 redTile)
        {
            int m5rCount;
            int p5rCount;
            int s5rCount;
            switch (redTile)
            {
                case Rules.赤牌.赤ドラ3:
                    m5rCount = 1;
                    p5rCount = 1;
                    s5rCount = 1;
                    break;
                case Rules.赤牌.赤ドラ4:
                    m5rCount = 1;
                    p5rCount = 2;
                    s5rCount = 1;
                    break;
                case Rules.赤牌.赤無し:
                    m5rCount = 0;
                    p5rCount = 0;
                    s5rCount = 0;
                    break;
                default:
                    throw new System.ArgumentException(redTile.ToString());
            }

            var sortedTiles = new List<Tile>();
            foreach (var it in TileTypeUtil.CreateSet(花牌: false, 季節牌: false))
            {
                if (it == TileType.M5 && m5rCount > 0)
                {
                    --m5rCount;
                    sortedTiles.Add(new Tile(sortedTiles.Count, it, true));
                }
                else if (it == TileType.P5 && p5rCount > 0)
                {
                    --p5rCount;
                    sortedTiles.Add(new Tile(sortedTiles.Count, it, true));
                }
                else if (it == TileType.S5 && s5rCount > 0)
                {
                    --s5rCount;
                    sortedTiles.Add(new Tile(sortedTiles.Count, it, true));
                }
                else
                {
                    sortedTiles.Add(new Tile(sortedTiles.Count, it, false));
                }
            }
            allTiles = sortedTiles.ToArray();
            tiles = sortedTiles.ToList();

            var random = RandomProvider.GetNewRandom();
            randomSeed = random.seed;
            random.Shuffle(ref tiles);
        }

        壁牌(in Serializables.壁牌 source)
        {
            allTiles = source.allTiles
                .Select(_ => _.Deserialize())
                .ToArray();
            tiles = source.tiles
                .Select(_ => allTiles[_])
                .ToList();
            randomSeed = source.randomSeed;
        }

        static public 壁牌 FromSerializable(in Serializables.壁牌 source)
        {
            return new 壁牌(source);
        }
        public Serializables.壁牌 ToSerializable()
        {
            return new Serializables.壁牌(this);
        }
    }

    public class 王牌
    {
        public const int Count = 14;
        readonly public List<Tile> tiles = new();
        readonly public List<Tile> ドラ表示牌 = new();
        readonly public List<Tile> 裏ドラ表示牌 = new();
        public int DrawnCount { get; private set; }
        public int Remaining嶺上牌Count => 4 - DrawnCount;

        public 王牌()
        {
        }

        王牌(in Serializables.王牌 source, 壁牌 wallTile)
        {
            tiles = source.tiles.Select(_ => wallTile.allTiles[_]).ToList();
            ドラ表示牌 = source.ドラ表示牌.Select(_ => wallTile.allTiles[_]).ToList();
            裏ドラ表示牌 = source.裏ドラ表示牌.Select(_ => wallTile.allTiles[_]).ToList();
            DrawnCount = source.drawnCount;
        }

        public static 王牌 FromSerializable(in Serializables.王牌 source, 壁牌 wallTile)
        {
            return new 王牌(source, wallTile);
        }

        public Serializables.王牌 ToSerializable()
        {
            return new Serializables.王牌(this);
        }

        public void OpenDora()
        {
            {
                var t = tiles[0];
                tiles.RemoveAt(0);
                ドラ表示牌.Add(t);
            }
            {
                var t = tiles[0];
                tiles.RemoveAt(0);
                裏ドラ表示牌.Add(t);
            }
        }

        public Tile Draw()
        {
            ++DrawnCount;
            var result = tiles[0];
            tiles.RemoveAt(0);
            return result;
        }

        public TileType[] DoraTiles => ドラ表示牌.Select(_ => TileTypeUtil.GetDora(_.type)).ToArray();
        public TileType[] UraDoraTiles => 裏ドラ表示牌.Select(_ => TileTypeUtil.GetDora(_.type)).ToArray();
    }

}
