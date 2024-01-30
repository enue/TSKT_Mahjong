#nullable enable
using System.Linq;
using UnityEngine;

namespace TSKT.Mahjongs.Serializables
{
    [System.Serializable]
    public struct Session
    {
        public bool hasAfterDiscard;
        public AfterDiscard afterDiscard;
        public bool hasAfterDraw;
        public AfterDraw afterDraw;
        public bool hasBefore暗槓;
        public Before暗槓 before暗槓;
        public bool hasBefore加槓;
        public Before加槓 before加槓;

        public Session(Mahjongs.AfterDiscard source)
        {
            hasAfterDiscard = true;
            hasAfterDraw = false;
            hasBefore加槓 = false;
            hasBefore暗槓 = false;

            afterDiscard = source.ToSerializable();
            afterDraw = default;
            before加槓 = default;
            before暗槓 = default;
        }

        public Session(Mahjongs.AfterDraw source)
        {
            hasAfterDiscard = false;
            hasAfterDraw = true;
            hasBefore加槓 = false;
            hasBefore暗槓 = false;

            afterDiscard = default;
            afterDraw = source.ToSerializable();
            before加槓 = default;
            before暗槓 = default;
        }

        public Session(Mahjongs.Before加槓 source)
        {
            hasAfterDiscard = false;
            hasAfterDraw = false;
            hasBefore加槓 = true;
            hasBefore暗槓 = false;

            afterDiscard = default;
            afterDraw = default;
            before加槓 = source.ToSerializable();
            before暗槓 = default;
        }

        public Session(Mahjongs.Before暗槓 source)
        {
            hasAfterDiscard = false;
            hasAfterDraw = false;
            hasBefore加槓 = false;
            hasBefore暗槓 = true;

            afterDiscard = default;
            afterDraw = default;
            before加槓 = default;
            before暗槓 = source.ToSerializable();
        }

        readonly public IController Deserialize()
        {
            if (hasAfterDiscard)
            {
                return afterDiscard.Deserialize();
            }
            if (hasAfterDraw)
            {
                return afterDraw.Deserialize();
            }
            if (hasBefore加槓)
            {
                return before加槓.Deserialize();
            }
            if (hasBefore暗槓)
            {
                return before暗槓.Deserialize();
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
        readonly public Mahjongs.Tile Deserialize()
        {
            return Mahjongs.Tile.FromSerializable(this);
        }
    }

    [System.Serializable]
    public struct Game
    {
        public int 場風Index;
        public int display局;
        public int リーチ棒スコア;
        public int 本場;
        public int 連荘;
        public PlayerIndex 起家;
        public int[] scores;
        public RuleSetting rule;

        public Game(Mahjongs.Game source)
        {
            display局 = source.Display局;
            起家 = source.起家;
            リーチ棒スコア = source.リーチ棒スコア;
            場風Index = source.場風Index;
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
    public struct 局
    {
        public Game game;
        public 壁牌 壁牌;
        public 王牌 王牌;
        public Player[] players;
        public TileType 場風;
        public PlayerIndex 親;
        public int[] totalDiscardedTiles;

        public 局(Mahjongs.局 source)
        {
            game = source.game.ToSerializable();
            王牌 = source.王牌.ToSerializable();
            親 = source.親Index;
            players = source.players.Select(_ => _.ToSerializable()).ToArray();
            場風 = source.場風;
            totalDiscardedTiles = source.totalDiscardedTiles.Select(_ => _.index).ToArray();
            壁牌 = source.壁牌.ToSerializable();
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
        public 手牌 手牌;
        public int[] 河;
        public int[] 捨て牌;
        public TileType 自風;
        public int riichiIndexInDiscardPile;
        public int riichiIndexInTotalDiscardTiles;

        public bool ダブルリーチ;
        public bool オープンリーチ;
        public bool 一発;
        public bool フリテンByOtherPlayers;

        public Player(Rounds.Player source)
        {
            index = source.index;
            捨て牌 = source.捨て牌.Select(_ => _.index).ToArray();
            河 = source.河.Select(_ => _.index).ToArray();
            ダブルリーチ = source.ダブルリーチ;
            手牌 = source.手牌.ToSerializable();
            オープンリーチ = source.オープンリーチ;
            riichiIndexInDiscardPile = source.RiichiIndexInDiscardPile ?? -1;
            riichiIndexInTotalDiscardTiles = source.RiichiIndexInTotalDiscardTiles ?? -1;
            自風 = source.自風;
            フリテンByOtherPlayers = source.フリテンByOtherPlayers;
            一発 = source.一発;
        }

        public readonly Rounds.Player Deserialize(Mahjongs.局 round)
        {
            return Rounds.Player.FromSerializable(this, round);
        }
    }

    [System.Serializable]
    public struct 手牌
    {
        public int[] tiles;
        public 副露[] 副露;

        public 手牌(Mahjongs.手牌 source)
        {
            tiles = source.tiles.Select(_ => _.index).ToArray();
            副露 = source.副露.Select(_ => _.ToSerializable()).ToArray();
        }

        public readonly Mahjongs.手牌 Deserialize(Rounds.Player owner)
        {
            return Mahjongs.手牌.FromSerializable(this, owner);
        }
    }

    [System.Serializable]
    public struct 副露
    {
        [System.Serializable]
        public struct Pair
        {
            public int tile;
            public PlayerIndex fromPlayerIndex;
        }
        public Pair[] tileFroms;

        public 副露(Mahjongs.副露 source)
        {
            tileFroms = source.tileFroms
                .Select(_ => new Pair() { tile = _.tile.index, fromPlayerIndex = _.fromPlayerIndex })
                .ToArray();
        }

        public readonly Mahjongs.副露 Deserialize(Mahjongs.壁牌 wallTile)
        {
            return Mahjongs.副露.FromSerializable(this, wallTile);
        }
    }

    [System.Serializable]
    public struct 壁牌
    {
        public int[] tiles;
        public Tile[] allTiles;
        public uint randomSeed;

        public 壁牌(Mahjongs.壁牌 source)
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
    public struct 王牌
    {
        public int[] tiles;
        public int[] ドラ表示牌;
        public int[] 裏ドラ表示牌;
        public int drawnCount;

        public 王牌(Mahjongs.王牌 source)
        {
            ドラ表示牌 = source.ドラ表示牌.Select(_ => _.index).ToArray();
            drawnCount = source.DrawnCount;
            tiles = source.tiles.Select(_ => _.index).ToArray();
            裏ドラ表示牌 = source.裏ドラ表示牌.Select(_ => _.index).ToArray();
        }

        public readonly Mahjongs.王牌 Deserialize(Mahjongs.壁牌 wallTile)
        {
            return Mahjongs.王牌.FromSerializable(this, wallTile);
        }
    }
}
