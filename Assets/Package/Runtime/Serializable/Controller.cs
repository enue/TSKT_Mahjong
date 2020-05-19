using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs.Serializables
{
    [SerializeField]
    public struct AfterDiscard
    {
        public int discardPlayerIndex;
    }

    [SerializeField]
    public struct AfterDraw
    {
        public int drawPlayerIndex;
        public int newTileInHand;
        public bool openDoraAfterDiscard;
        public bool 嶺上;
    }

    [SerializeField]
    public struct BeforeAddedOpenQuad
    {
        public int declarePlayerIndex;
        public int tile;
    }

    [SerializeField]
    public struct BeforeClosedQuad
    {
        public int declarePlayerIndex;
        public TileType tile;
    }
}
