using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class Hand
    {
        readonly Player owner;
        readonly public List<Tile> tiles = new List<Tile>();
        readonly public List<Meld> melds = new List<Meld>();

        public Hand(Player owner)
        {
            this.owner = owner;
        }

        public Serializables.Hand ToSerializable()
        {
            var result = new Serializables.Hand();
            result.tiles = tiles.Select(_ => _.id).ToArray();
            result.melds = melds.Select(_ => _.ToSerializable()).ToArray();
            return result;
        }

        public void Sort()
        {
            tiles.Sort();
        }

        public Hand Clone()
        {
            var result = new Hand(owner);
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

        public void BuildClosedQuad(TileType tileType)
        {
            var meld = new Meld();
            melds.Add(meld);
            for (int i = 0; i < 4; ++i)
            {
                var tile = tiles.First(_ => _.type == tileType);
                tiles.Remove(tile);
                meld.tileFroms.Add((tile, owner));
            }
        }

        public TileType[] GetWinningTiles()
        {
            var solution = Solve();
            if (solution.向聴数 != 0)
            {
                return System.Array.Empty<TileType>();
            }

            var result = new List<TileType>();
            foreach (TileType tile in System.Enum.GetValues(typeof(TileType)))
            {
                if (tile.季節牌())
                {
                    continue;
                }
                if (tile.花牌())
                {
                    continue;
                }
                if (AllTiles.Count(_ => _.type == tile) == 4)
                {
                    continue;
                }

                var clone = Clone();
                clone.tiles.Add(new Tile(0, tile, false));
                if (clone.Solve().向聴数 == -1)
                {
                    result.Add(tile);
                }
            }

            return result.ToArray();
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
        public Serializables.Meld ToSerializable()
        {
            var result = new Serializables.Meld();
            result.tileFroms = tileFroms
                .Select(_ => new Serializables.Meld.Pair() { tile = _.tile.id, from = _.from.index })
                .ToArray();
            return result;
        }
    }
}