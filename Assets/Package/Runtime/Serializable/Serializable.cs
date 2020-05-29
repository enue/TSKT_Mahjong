using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs.Serializables
{
    [System.Serializable]
    public struct Session
    {
        public bool hasAfterDiscard;
        public AfterDiscard afterDiscard;
        public bool hasAfterDraw;
        public AfterDraw afterDraw;
        public bool hasBeforeClosedQuad;
        public BeforeClosedQuad beforeClosedQuad;
        public bool hasBeforeAddedOpenQuad;
        public BeforeAddedOpenQuad beforeAddedOpenQuad;

        public Session(Mahjongs.AfterDiscard source)
        {
            hasAfterDiscard = true;
            hasAfterDraw = false;
            hasBeforeAddedOpenQuad = false;
            hasBeforeClosedQuad = false;

            afterDiscard = source.ToSerializable();
            afterDraw = default;
            beforeAddedOpenQuad = default;
            beforeClosedQuad = default;
        }

        public Session(Mahjongs.AfterDraw source)
        {
            hasAfterDiscard = false;
            hasAfterDraw = true;
            hasBeforeAddedOpenQuad = false;
            hasBeforeClosedQuad = false;

            afterDiscard = default;
            afterDraw = source.ToSerializable();
            beforeAddedOpenQuad = default;
            beforeClosedQuad = default;
        }

        public Session(Mahjongs.BeforeAddedOpenQuad source)
        {
            hasAfterDiscard = false;
            hasAfterDraw = false;
            hasBeforeAddedOpenQuad = true;
            hasBeforeClosedQuad = false;

            afterDiscard = default;
            afterDraw = default;
            beforeAddedOpenQuad = source.ToSerializable();
            beforeClosedQuad = default;
        }

        public Session(Mahjongs.BeforeClosedQuad source)
        {
            hasAfterDiscard = false;
            hasAfterDraw = false;
            hasBeforeAddedOpenQuad = false;
            hasBeforeClosedQuad = true;

            afterDiscard = default;
            afterDraw = default;
            beforeAddedOpenQuad = default;
            beforeClosedQuad = source.ToSerializable();
        }

        public IController Deserialize()
        {
            if (hasAfterDiscard)
            {
                return afterDiscard.Deserialzie();
            }
            if (hasAfterDraw)
            {
                return afterDraw.Deserialzie();
            }
            if (hasBeforeAddedOpenQuad)
            {
                return beforeAddedOpenQuad.Deserialzie();
            }
            if (hasBeforeClosedQuad)
            {
                return beforeClosedQuad.Deserialzie();
            }
            throw new System.Exception("no controller");
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }
        public static IController FromJson(string json)
        {
            return JsonUtility.FromJson<Session>(json).Deserialize();
        }
    }

    [System.Serializable]
    public struct Tile
    {
        public int index;
        public TileType type;
        public bool red;

        public Tile(Mahjongs.Tile source)
        {
            index = source.index;
            red = source.red;
            type = source.type;
        }
        public Mahjongs.Tile Deserialzie()
        {
            return Mahjongs.Tile.FromSerializable(this);
        }
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

        public Game(Mahjongs.Game source)
        {
            displayRoundCount = source.DisplayRoundCount;
            firstDealer = source.firstDealer;
            riichiScore = source.riichiScore;
            roundWindCount = source.RoundWindCount;
            rule = source.rule;
            scores = source.scoreOwners.Select(_ => _.score).ToArray();
            本場 = source.本場;
            連荘 = source.連荘;
        }

        public Mahjongs.Game Deserialzie()
        {
            return Mahjongs.Game.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct Round
    {
        public Game game;
        public WallTile wallTile;
        public DeadWallTile deadWallTile;
        public Player[] players;
        public TileType roundWind;
        public int dealer;
        public int[] totalDiscardedTiles;

        public Round(Mahjongs.Round source)
        {
            game = source.game.ToSerializable();
            deadWallTile = source.deadWallTile.ToSerializable();
            dealer = source.dealer;
            players = source.players.Select(_ => _.ToSerializable()).ToArray();
            roundWind = source.roundWind;
            totalDiscardedTiles = source.totalDiscardedTiles.Select(_ => _.index).ToArray();
            wallTile = source.wallTile.ToSerializable();
        }

        public Mahjongs.Round Deserialzie()
        {
            return Mahjongs.Round.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct Player
    {
        public int index;
        public Hand hand;
        public int[] discardPile;
        public int[] discardedTiles;
        public TileType wind;
        public int riichiIndexInDiscardPile;
        public int riichiIndexInTotalDiscardTiles;

        public bool doubleRiichi;
        public bool openRiichi;
        public bool 一発;
        public bool furitenByOtherPlayers;

        public Player(Mahjongs.Player source)
        {
            index = source.index;
            discardedTiles = source.discardedTiles.Select(_ => _.index).ToArray();
            discardPile = source.discardPile.Select(_ => _.index).ToArray();
            doubleRiichi = source.DoubleRiichi;
            hand = source.hand.ToSerializable();
            openRiichi = source.OpenRiichi;
            riichiIndexInDiscardPile = source.RiichiIndexInDiscardPile ?? -1;
            riichiIndexInTotalDiscardTiles = source.RiichiIndexInTotalDiscardTiles ?? -1;
            wind = source.wind;
            furitenByOtherPlayers = source.FuritenByOtherPlayers;
            一発 = source.一発;
        }

        public Mahjongs.Player Deserialzie(Mahjongs.Round round)
        {
            return Mahjongs.Player.FromSerializable(this, round);
        }
    }

    [System.Serializable]
    public struct Hand
    {
        public int[] tiles;
        public Meld[] melds;

        public Hand(Mahjongs.Hand source)
        {
            tiles = source.tiles.Select(_ => _.index).ToArray();
            melds = source.melds.Select(_ => _.ToSerializable()).ToArray();
        }

        public Mahjongs.Hand Deserialzie(Mahjongs.Player owner)
        {
            return Mahjongs.Hand.FromSerializable(this, owner);
        }
    }

    [System.Serializable]
    public struct Meld
    {
        [System.Serializable]
        public struct Pair
        {
            public int tile;
            public int fromPlayerIndex;
        }
        public Pair[] tileFroms;

        public Meld(Mahjongs.Meld source)
        {
            tileFroms = source.tileFroms
                .Select(_ => new Pair() { tile = _.tile.index, fromPlayerIndex = _.fromPlayerIndex })
                .ToArray();
        }

        public Mahjongs.Meld Deserialzie(Mahjongs.WallTile wallTile)
        {
            return Mahjongs.Meld.FromSerializable(this, wallTile);
        }
    }

    [System.Serializable]
    public struct WallTile
    {
        public int[] tiles;
        public Tile[] allTiles;
        public uint randomSeed;

        public WallTile(Mahjongs.WallTile source)
        {
            allTiles = source.allTiles.Select(_ => _.ToSerializable()).ToArray();
            tiles = source.tiles.Select(_ => _.index).ToArray();
            randomSeed = source.randomSeed;
        }

        public Mahjongs.WallTile Deserialzie()
        {
            return Mahjongs.WallTile.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct DeadWallTile
    {
        public int[] tiles;
        public int[] doraIndicatorTiles;
        public int[] uraDoraIndicatorTiles;
        public int drawnCount;

        public DeadWallTile(Mahjongs.DeadWallTile source)
        {
            doraIndicatorTiles = source.doraIndicatorTiles.Select(_ => _.index).ToArray();
            drawnCount = source.DrawnCount;
            tiles = source.tiles.Select(_ => _.index).ToArray();
            uraDoraIndicatorTiles = source.uraDoraIndicatorTiles.Select(_ => _.index).ToArray();
        }

        public Mahjongs.DeadWallTile Deserialzie(Mahjongs.WallTile wallTile)
        {
            return Mahjongs.DeadWallTile.FromSerializable(this, wallTile);
        }
    }
}
