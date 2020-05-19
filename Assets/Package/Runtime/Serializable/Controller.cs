using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs.Serializables
{
    [SerializeField]
    public struct AfterDiscard
    {
        public Round round;

        public int discardPlayerIndex;

        public AfterDiscard(Mahjongs.AfterDiscard source)
        {
            round = source.Round.ToSerializable();

            discardPlayerIndex = source.DiscardPlayerIndex;
        }
    }

    [SerializeField]
    public struct AfterDraw
    {
        public Round round;

        public int drawPlayerIndex;
        public int newTileInHand;
        public bool openDoraAfterDiscard;
        public bool 嶺上;

        public AfterDraw(Mahjongs.AfterDraw source)
        {
            round = source.Round.ToSerializable();

            drawPlayerIndex = source.DrawPlayerIndex;
            newTileInHand = source.newTileInHand?.id ?? -1;
            openDoraAfterDiscard = source.openDoraAfterDiscard;
            嶺上 = source.嶺上;
        }
    }

    [SerializeField]
    public struct BeforeAddedOpenQuad
    {
        public Round round;

        public int declarePlayerIndex;
        public int tile;
        
        public BeforeAddedOpenQuad(Mahjongs.BeforeAddedOpenQuad source)
        {
            round = source.Round.ToSerializable();

            declarePlayerIndex = source.DeclarePlayerIndex;
            tile = source.tile.id;
        }
    }

    [SerializeField]
    public struct BeforeClosedQuad
    {
        public Round round;

        public int declarePlayerIndex;
        public TileType tile;

        public BeforeClosedQuad(Mahjongs.BeforeClosedQuad source)
        {
            round = source.Round.ToSerializable();

            declarePlayerIndex = source.DeclarePlayerIndex;
            tile = source.tile;
        }
    }
}
