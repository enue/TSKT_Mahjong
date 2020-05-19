using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs.Serializables
{
    [System.Serializable]
    public struct AfterDiscard
    {
        public Round round;

        public int discardPlayerIndex;

        public AfterDiscard(Mahjongs.AfterDiscard source)
        {
            round = source.Round.ToSerializable();

            discardPlayerIndex = source.DiscardPlayerIndex;
        }
        public Mahjongs.AfterDiscard Deserialzie()
        {
            return Mahjongs.AfterDiscard.FromSerializable(this);
        }
    }

    [System.Serializable]
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
            newTileInHand = source.newTileInHand?.index ?? -1;
            openDoraAfterDiscard = source.openDoraAfterDiscard;
            嶺上 = source.嶺上;
        }
        public Mahjongs.AfterDraw Deserialzie()
        {
            return Mahjongs.AfterDraw.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct BeforeAddedOpenQuad
    {
        public Round round;

        public int declarePlayerIndex;
        public int tile;
        
        public BeforeAddedOpenQuad(Mahjongs.BeforeAddedOpenQuad source)
        {
            round = source.Round.ToSerializable();

            declarePlayerIndex = source.DeclarePlayerIndex;
            tile = source.tile.index;
        }
        public Mahjongs.BeforeAddedOpenQuad Deserialzie()
        {
            return Mahjongs.BeforeAddedOpenQuad.FromSerializable(this);
        }
    }

    [System.Serializable]
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
        public Mahjongs.BeforeClosedQuad Deserialzie()
        {
            return Mahjongs.BeforeClosedQuad.FromSerializable(this);
        }
    }
}
