﻿#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TSKT.Mahjongs.Rounds;

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

        public Session(Mahjongs.Before加槓 source)
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

        public Session(Mahjongs.Before暗槓 source)
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

        readonly public IController Deserialize()
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

        readonly public string ToJson(bool prettyPrint = false)
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
        readonly public Mahjongs.Tile Deserialzie()
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
        public PlayerIndex firstDealer;
        public int[] scores;
        public RuleSetting rule;

        public Game(Mahjongs.Game source)
        {
            displayRoundCount = source.DisplayRoundCount;
            firstDealer = source.起家;
            riichiScore = source.リーチ棒スコア;
            roundWindCount = source.RoundWindCount;
            rule = source.rule;
            scores = source.seats.Select(_ => _.score).ToArray();
            本場 = source.本場;
            連荘 = source.連荘;
        }

        public readonly Mahjongs.Game Deserialize()
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
        public PlayerIndex dealer;
        public int[] totalDiscardedTiles;

        public Round(Mahjongs.局 source)
        {
            game = source.game.ToSerializable();
            deadWallTile = source.王牌.ToSerializable();
            dealer = source.dealer;
            players = source.players.Select(_ => _.ToSerializable()).ToArray();
            roundWind = source.場風;
            totalDiscardedTiles = source.totalDiscardedTiles.Select(_ => _.index).ToArray();
            wallTile = source.壁牌.ToSerializable();
        }

        public readonly Mahjongs.局 Deserialize()
        {
            return Mahjongs.局.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct Player
    {
        public PlayerIndex index;
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

        public Player(Rounds.Player source)
        {
            index = source.index;
            discardedTiles = source.捨て牌.Select(_ => _.index).ToArray();
            discardPile = source.河.Select(_ => _.index).ToArray();
            doubleRiichi = source.ダブルリーチ;
            hand = source.手牌.ToSerializable();
            openRiichi = source.オープンリーチ;
            riichiIndexInDiscardPile = source.RiichiIndexInDiscardPile ?? -1;
            riichiIndexInTotalDiscardTiles = source.RiichiIndexInTotalDiscardTiles ?? -1;
            wind = source.自風;
            furitenByOtherPlayers = source.フリテンByOtherPlayers;
            一発 = source.一発;
        }

        public readonly Rounds.Player Deserialize(Mahjongs.局 round)
        {
            return Rounds.Player.FromSerializable(this, round);
        }
    }

    [System.Serializable]
    public struct Hand
    {
        public int[] tiles;
        public Meld[] melds;

        public Hand(Mahjongs.手牌 source)
        {
            tiles = source.tiles.Select(_ => _.index).ToArray();
            melds = source.副露.Select(_ => _.ToSerializable()).ToArray();
        }

        public readonly Mahjongs.手牌 Deserialize(Rounds.Player owner)
        {
            return Mahjongs.手牌.FromSerializable(this, owner);
        }
    }

    [System.Serializable]
    public struct Meld
    {
        [System.Serializable]
        public struct Pair
        {
            public int tile;
            public PlayerIndex fromPlayerIndex;
        }
        public Pair[] tileFroms;

        public Meld(Mahjongs.副露 source)
        {
            tileFroms = source.tileFroms
                .Select(_ => new Pair() { tile = _.tile.index, fromPlayerIndex = _.fromPlayerIndex })
                .ToArray();
        }

        public Mahjongs.副露 Deserialize(Mahjongs.壁牌 wallTile)
        {
            return Mahjongs.副露.FromSerializable(this, wallTile);
        }
    }

    [System.Serializable]
    public struct WallTile
    {
        public int[] tiles;
        public Tile[] allTiles;
        public uint randomSeed;

        public WallTile(Mahjongs.壁牌 source)
        {
            allTiles = source.allTiles.Select(_ => _.ToSerializable()).ToArray();
            tiles = source.tiles.Select(_ => _.index).ToArray();
            randomSeed = source.randomSeed;
        }

        public readonly Mahjongs.壁牌 Deserialize()
        {
            return Mahjongs.壁牌.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct DeadWallTile
    {
        public int[] tiles;
        public int[] doraIndicatorTiles;
        public int[] uraDoraIndicatorTiles;
        public int drawnCount;

        public DeadWallTile(Mahjongs.王牌 source)
        {
            doraIndicatorTiles = source.ドラ表示牌.Select(_ => _.index).ToArray();
            drawnCount = source.DrawnCount;
            tiles = source.tiles.Select(_ => _.index).ToArray();
            uraDoraIndicatorTiles = source.裏ドラ表示牌.Select(_ => _.index).ToArray();
        }

        public readonly Mahjongs.王牌 Deserialize(Mahjongs.壁牌 wallTile)
        {
            return Mahjongs.王牌.FromSerializable(this, wallTile);
        }
    }
}
