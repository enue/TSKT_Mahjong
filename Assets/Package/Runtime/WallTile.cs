using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class WallTile
    {
        readonly public List<Tile> tiles = new List<Tile>();

        public WallTile(Rules.RedTile redTile)
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

            foreach (var it in TileTypeUtil.CreateSet(花牌: false, 季節牌: false))
            {
                if (it == TileType.M5 && m5rCount > 0)
                {
                    --m5rCount;
                    tiles.Add(new Tile(it, true));
                }
                else if (it == TileType.P5 && p5rCount > 0)
                {
                    --p5rCount;
                    tiles.Add(new Tile(it, true));
                }
                else if (it == TileType.S5 && s5rCount > 0)
                {
                    --s5rCount;
                    tiles.Add(new Tile(it, true));
                }
                else
                {
                    tiles.Add(new Tile(it, false));
                }
            }
            RandomUtil.Shuffle(ref tiles);
        }
    }

    public class DeadWallTile
    {
        public const int Count = 14;
        readonly public List<Tile> tiles = new List<Tile>();
        readonly public List<Tile> doraIndicatorTiles = new List<Tile>();
        readonly public List<Tile> uraDoraIndicatorTiles = new List<Tile>();
        public int DrawnCount { get; private set; }
        public int RemainingReplacementTileCount => 4 - DrawnCount;

        public void OpenDora()
        {
            {
                var t = tiles[0];
                tiles.RemoveAt(0);
                doraIndicatorTiles.Add(t);
            }
            {
                var t = tiles[0];
                tiles.RemoveAt(0);
                uraDoraIndicatorTiles.Add(t);
            }
        }

        public Tile Draw()
        {
            ++DrawnCount;
            var result = tiles[0];
            tiles.RemoveAt(0);
            return result;
        }

        public TileType[] DoraTiles => doraIndicatorTiles.Select(_ => TileTypeUtil.GetDora(_.type)).ToArray();
        public TileType[] UraDoraTiles => uraDoraIndicatorTiles.Select(_ => TileTypeUtil.GetDora(_.type)).ToArray();
    }

}
