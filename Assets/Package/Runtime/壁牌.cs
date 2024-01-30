using System.Collections;
using System.Collections.Generic;
using System.Linq;
#nullable enable

namespace TSKT.Mahjongs
{
    public class 壁牌
    {
        readonly public uint randomSeed;
        readonly public List<Tile> tiles = new List<Tile>();
        readonly public Tile[] allTiles;

        public 壁牌(Rules.RedTile redTile)
        {
            int m5rCount;
            int p5rCount;
            int s5rCount;
            switch (redTile)
            {
                case Rules.RedTile.赤ドラ3:
                    m5rCount = 1;
                    p5rCount = 1;
                    s5rCount = 1;
                    break;
                case Rules.RedTile.赤ドラ4:
                    m5rCount = 1;
                    p5rCount = 2;
                    s5rCount = 1;
                    break;
                case Rules.RedTile.赤無し:
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

        壁牌(in Serializables.WallTile source)
        {
            allTiles = source.allTiles
                .Select(_ => _.Deserialzie())
                .ToArray();
            tiles = source.tiles
                .Select(_ => allTiles[_])
                .ToList();
            randomSeed = source.randomSeed;
        }

        static public 壁牌 FromSerializable(in Serializables.WallTile source)
        {
            return new 壁牌(source);
        }
        public Serializables.WallTile ToSerializable()
        {
            return new Serializables.WallTile(this);
        }
    }

    public class 王牌
    {
        public const int Count = 14;
        readonly public List<Tile> tiles = new();
        readonly public List<Tile> ドラ表示牌 = new();
        readonly public List<Tile> 裏ドラ表示牌 = new();
        public int DrawnCount { get; private set; }
        public int RemainingReplacementTileCount => 4 - DrawnCount;

        public 王牌()
        {
        }

        王牌(in Serializables.DeadWallTile source, 壁牌 wallTile)
        {
            tiles = source.tiles.Select(_ => wallTile.allTiles[_]).ToList();
            ドラ表示牌 = source.doraIndicatorTiles.Select(_ => wallTile.allTiles[_]).ToList();
            裏ドラ表示牌 = source.uraDoraIndicatorTiles.Select(_ => wallTile.allTiles[_]).ToList();
            DrawnCount = source.drawnCount;
        }

        public static 王牌 FromSerializable(in Serializables.DeadWallTile source, 壁牌 wallTile)
        {
            return new 王牌(source, wallTile);
        }

        public Serializables.DeadWallTile ToSerializable()
        {
            return new Serializables.DeadWallTile(this);
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
