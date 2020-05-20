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

        Hand(Serializables.Hand source, Player owner)
        {
            this.owner = owner;
            melds = source.melds.Select(_ => _.Deserialzie(owner.round.wallTile)).ToList();
            tiles = source.tiles.Select(_ => owner.round.wallTile.allTiles[_]).ToList();
        }

        static public Hand FromSerializable(Serializables.Hand source, Player owner)
        {
            return new Hand(source, owner);
        }
        public Serializables.Hand ToSerializable()
        {
            return new Serializables.Hand(this);
        }

        public void Sort()
        {
            tiles.Sort();
        }

        public Hand Clone()
        {
            var result = new Hand(owner);
            result.tiles.AddRange(tiles);
            result.melds.AddRange(melds.Select(_ => _.Clone()));
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
                meld.tileFroms.Add((tile, owner.index));
            }
        }

        public TileType[] GetWinningTiles()
        {
            if (!向聴数IsLessThanOrEqual(0))
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
                if (clone.向聴数IsLessThanOrEqual(-1))
                {
                    result.Add(tile);
                }
            }

            return result.ToArray();
        }
    }

    public class Meld
    {
        public readonly List<(Tile tile, int fromPlayerIndex)> tileFroms = new List<(Tile, int)>();

        public bool 順子 => tileFroms[0].tile.type != tileFroms[1].tile.type;
        public bool 槓子 => tileFroms.Count == 4;
        public bool 暗槓 => 槓子 && tileFroms.All(_ => _.fromPlayerIndex == tileFroms[0].fromPlayerIndex);

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

        public Meld()
        {
        }
        
        Meld(Serializables.Meld source, WallTile wallTile)
        {
            tileFroms = source.tileFroms.Select(_ => (wallTile.allTiles[_.tile], _.fromPlayerIndex)).ToList();
        }

        public Meld Clone()
        {
            var result = new Meld();
            result.tileFroms.AddRange(tileFroms);
            return result;
        }

        static public Meld FromSerializable(Serializables.Meld source, WallTile wallTile)
        {
            return new Meld(source, wallTile);
        }

        public Serializables.Meld ToSerializable()
        {
            return new Serializables.Meld(this);
        }

        public (Tile tile, int fromPlayerIndex) TileFromOtherPlayer
        {
            get
            {
                int ownerPlayerIndex;
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
                        return it;
                    }
                }
                return default;
            }
        }

        public bool Is喰い替え(Tile tileToDiscard)
        {
            var tileFromOtherPlayer = TileFromOtherPlayer;
            if (tileToDiscard.type == tileFromOtherPlayer.tile.type)
            {
                return true;
            }

            if (順子)
            {
                if (tileToDiscard.type.IsSuited())
                {
                    if (tileFroms[0].tile.type.Suit() == tileToDiscard.type.Suit())
                    {
                        var numbers = tileFroms
                            .Where(_ => _.fromPlayerIndex != tileFromOtherPlayer.fromPlayerIndex)
                            .Select(_ => _.tile.type.Number())
                            .Concat(new[] { tileToDiscard.type.Number() })
                            .Distinct()
                            .OrderBy(_ => _)
                            .ToArray();
                        if (numbers.Length == 3
                            && numbers[2] - numbers[0] == 2)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
