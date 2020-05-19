using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs
{
    public class Tile : System.IComparable<Tile>
    {
        readonly public int index;
        readonly public TileType type;
        readonly public bool red;

        public Tile(int index, TileType type, bool red)
        {
            this.index = index;
            this.type = type;
            this.red = red;
        }

        static public Tile FromSerializable(Serializables.Tile source)
        {
            return new Tile(source.index, source.type, source.red);
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
            result.index = index;
            result.red = red;
            result.type = type;
            return result;
        }
    }
}
