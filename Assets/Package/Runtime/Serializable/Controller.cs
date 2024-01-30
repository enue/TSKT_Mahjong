using System.Collections;
using System.Collections.Generic;
#nullable enable

namespace TSKT.Mahjongs.Serializables
{
    [System.Serializable]
    public struct AfterDiscard
    {
        public 局 局;

        public PlayerIndex discardPlayerIndex;

        public AfterDiscard(Mahjongs.AfterDiscard source)
        {
            局 = source.局.ToSerializable();

            discardPlayerIndex = source.DiscardPlayerIndex;
        }
        readonly public Mahjongs.AfterDiscard Deserialize()
        {
            return Mahjongs.AfterDiscard.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct AfterDraw
    {
        public 局 局;

        public PlayerIndex drawPlayerIndex;
        public int newTileInHand;
        public bool openDoraAfterDiscard;
        public bool 嶺上;

        public AfterDraw(Mahjongs.AfterDraw source)
        {
            局 = source.局.ToSerializable();

            drawPlayerIndex = source.DrawPlayerIndex;
            newTileInHand = source.newTileInHand?.index ?? -1;
            openDoraAfterDiscard = source.openDoraAfterDiscard;
            嶺上 = source.嶺上;
        }
        readonly public Mahjongs.AfterDraw Deserialize()
        {
            return Mahjongs.AfterDraw.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct Before加槓
    {
        public 局 局;

        public PlayerIndex declarePlayerIndex;
        public int tile;
        
        public Before加槓(Mahjongs.Before加槓 source)
        {
            局 = source.局.ToSerializable();

            declarePlayerIndex = source.DeclarePlayerIndex;
            tile = source.tile.index;
        }
        readonly public Mahjongs.Before加槓 Deserialize()
        {
            return Mahjongs.Before加槓.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct Before暗槓
    {
        public 局 局;

        public PlayerIndex declarePlayerIndex;
        public TileType tile;

        public Before暗槓(Mahjongs.Before暗槓 source)
        {
            局 = source.局.ToSerializable();

            declarePlayerIndex = source.DeclarePlayerIndex;
            tile = source.tile;
        }
        readonly public Mahjongs.Before暗槓 Deserialize()
        {
            return Mahjongs.Before暗槓.FromSerializable(this);
        }
    }
}
