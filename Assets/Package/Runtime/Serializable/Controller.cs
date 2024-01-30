using System.Collections;
using System.Collections.Generic;
#nullable enable

namespace TSKT.Mahjongs.Serializables
{
    [System.Serializable]
    public struct AfterDiscard
    {
        public Round round;

        public PlayerIndex discardPlayerIndex;

        public AfterDiscard(Mahjongs.AfterDiscard source)
        {
            round = source.局.ToSerializable();

            discardPlayerIndex = source.DiscardPlayerIndex;
        }
        readonly public Mahjongs.AfterDiscard Deserialzie()
        {
            return Mahjongs.AfterDiscard.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct AfterDraw
    {
        public Round round;

        public PlayerIndex drawPlayerIndex;
        public int newTileInHand;
        public bool openDoraAfterDiscard;
        public bool 嶺上;

        public AfterDraw(Mahjongs.AfterDraw source)
        {
            round = source.局.ToSerializable();

            drawPlayerIndex = source.DrawPlayerIndex;
            newTileInHand = source.newTileInHand?.index ?? -1;
            openDoraAfterDiscard = source.openDoraAfterDiscard;
            嶺上 = source.嶺上;
        }
        readonly public Mahjongs.AfterDraw Deserialzie()
        {
            return Mahjongs.AfterDraw.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct BeforeAddedOpenQuad
    {
        public Round round;

        public PlayerIndex declarePlayerIndex;
        public int tile;
        
        public BeforeAddedOpenQuad(Mahjongs.Before加槓 source)
        {
            round = source.局.ToSerializable();

            declarePlayerIndex = source.DeclarePlayerIndex;
            tile = source.tile.index;
        }
        readonly public Mahjongs.Before加槓 Deserialzie()
        {
            return Mahjongs.Before加槓.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct BeforeClosedQuad
    {
        public Round round;

        public PlayerIndex declarePlayerIndex;
        public TileType tile;

        public BeforeClosedQuad(Mahjongs.Before暗槓 source)
        {
            round = source.局.ToSerializable();

            declarePlayerIndex = source.DeclarePlayerIndex;
            tile = source.tile;
        }
        readonly public Mahjongs.Before暗槓 Deserialzie()
        {
            return Mahjongs.Before暗槓.FromSerializable(this);
        }
    }
}
