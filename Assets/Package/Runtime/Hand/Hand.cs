using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class Hand
    {
        readonly public List<Tile> tiles = new List<Tile>();
        readonly public List<Meld> melds = new List<Meld>();

        public void Sort()
        {
            tiles.Sort();
        }

        public Hand Clone()
        {
            var result = new Hand();
            result.tiles.AddRange(tiles);
            result.melds.AddRange(melds);
            return result;
        }

        public Hands.Solution Solve()
        {
            return new Hands.Solution(this);
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
                foreach (var meld in melds)
                {
                    foreach (var (tile, from) in meld.tileFroms)
                    {
                        yield return tile;
                    }
                }
            }
        }
    }

    public class Meld
    {
        public readonly List<(Tile tile, Player from)> tileFroms = new List<(Tile tile, Player from)>();

        public bool 順子 => tileFroms[0].tile != tileFroms[1].tile;
        public bool 槓子 => tileFroms.Count == 4;
        public bool 暗槓 => 槓子 && tileFroms.All(_ => _.from == tileFroms[0].from);

        public Tile[] Sorted => tileFroms.Select(_ => _.tile).OrderBy(_ => _.type).ToArray();

        public Tile Min
        {
            get
            {
                var result = tileFroms[0].tile;
                foreach (var (tile, from) in tileFroms)
                {
                    if (result.type > tile.type)
                    {
                        result = tile;
                    }
                }
                return result;
            }
        }
    }

}
