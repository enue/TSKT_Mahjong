using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs
{
    public class Tile : System.IComparable<Tile>
    {
        readonly public TileType type;
        readonly public bool red;

        public Tile(TileType type, bool red)
        {
            this.type = type;
            this.red = red;
        }

        public int CompareTo(Tile other)
        {
            if (type > other.type)
            {
                return 1;
            }
            if (type < other.type)
            {
                return -1;
            }
            return 0;
        }
    }
}
