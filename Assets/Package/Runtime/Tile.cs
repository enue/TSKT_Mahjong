using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs
{
    public class Tile : System.IComparable<Tile>
    {
        readonly public int id;
        readonly public TileType type;
        readonly public bool red;

        public Tile(int id, TileType type, bool red)
        {
            this.id = id;
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

        public Serializables.Tile ToSerializable()
        {
            var result = new Serializables.Tile();
            result.id = id;
            result.red = red;
            result.type = type;
            return result;
        }
    }
}
