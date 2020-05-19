using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs.Serializables
{
    [SerializeField]
    public struct AfterDiscard
    {
        public int discardPlayerIndex;

        public AfterDiscard(Mahjongs.AfterDiscard source)
        {
            discardPlayerIndex = source.DiscardPlayerIndex;
        }
    }

    [SerializeField]
    public struct AfterDraw
    {
        public int drawPlayerIndex;
        public int newTileInHand;
        public bool openDoraAfterDiscard;
        public bool 嶺上;

        public AfterDraw(Mahjongs.AfterDraw source)
        {
            drawPlayerIndex = source.DrawPlayerIndex;
            newTileInHand = source.newTileInHand?.id ?? -1;
            openDoraAfterDiscard = source.openDoraAfterDiscard;
            嶺上 = source.嶺上;
        }
    }

    [SerializeField]
    public struct BeforeAddedOpenQuad
    {
        public int declarePlayerIndex;
        public int tile;
        
        public BeforeAddedOpenQuad(Mahjongs.BeforeAddedOpenQuad source)
        {
            declarePlayerIndex = source.DeclarePlayerIndex;
            tile = source.tile.id;
        }
    }

    [SerializeField]
    public struct BeforeClosedQuad
    {
        public int declarePlayerIndex;
        public TileType tile;

        public BeforeClosedQuad(Mahjongs.BeforeClosedQuad source)
        {
            declarePlayerIndex = source.DeclarePlayerIndex;
            tile = source.tile;
        }
    }
}
