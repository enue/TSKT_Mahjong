using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs.Serializables
{
    [System.Serializable]
    public struct Tile
    {
        public int id;
        public TileType type;
        public bool red;
    }

    [System.Serializable]
    public struct Game
    {
        public int roundWindCount;
        public int displayRoundCount;
        public int riichiScore;
        public int 本場;
        public int 連荘;
        public int firstDealer;
        public int[] scores;
        public RuleSetting rule;
    }

    [System.Serializable]
    public struct Round
    {
        public WallTile wallTile;
        public DeadWallTile deadWallTile;
        public Player[] players;
        public TileType roundWind;
        public int dealer;
        public int[] totalDiscardedTiles;
    }

    [System.Serializable]
    public struct Player
    {
        public Hand hand;
        public int[] discardPile;
        public int[] discardedTiles;
        public TileType wind;
        public int riichiIndexInDiscardPile;
        public int riichiIndexInTotalDiscardTiles;

        public bool doubleRiichi;
        public bool openRiichi;
        public bool 一発;
        public bool フリテン;
    }

    [System.Serializable]
    public struct Hand
    {
        public int[] tiles;
        public Meld[] melds;
    }

    [System.Serializable]
    public struct Meld
    {
        [System.Serializable]
        public struct Pair
        {
            public int tile;
            public int from;
        }
        public Pair[] tileFroms;
    }

    [System.Serializable]
    public struct WallTile
    {
        public int[] tiles;
        public Tile[] allTiles;
    }

    [System.Serializable]
    public struct DeadWallTile
    {
        public int[] tiles;
        public int[] doraIndicatorTiles;
        public int[] uraDoraIndicatorTiles;
        public int drawnCount;
    }
}
