using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#nullable enable

namespace TSKT.Mahjongs
{
    public readonly struct CompletedHand
    {
        public readonly Hands.Structure structure;
        public readonly TileType[] IsolatedTiles => structure.IsolatedTiles;
        public readonly Hands.Structure.Set[] Sets => structure.Sets;
        public readonly TileType[] Pairs => structure.Pairs;
        public readonly Meld[] Melds => structure.melds;
        public readonly int RedTile => structure.redTileCount;

        public readonly Player? ronTarget;

        readonly TileType newTileInHand;
        readonly TileType ownWind;
        readonly TileType roundWind;
        public readonly Dictionary<役, int> Yakus;
        public readonly Dictionary<役, int> 役満;

        public readonly bool 役無し => Yakus.Count == 0 && 役満.Count == 0;
        public readonly int Han => Yakus.Values.Sum() + Dora + UraDora + RedTile;
        public readonly bool 面前;
        public readonly bool 自摸 => ronTarget == null;
        public readonly TileType[] doraTiles;
        public readonly TileType[] uraDoraTiles;
        readonly IEnumerable<TileType> AllUsedTiles => structure.AllUsedTiles;

        public CompletedHand(Hands.Structure structure, TileType newTileInHand, TileType ownWind, TileType roundWind,
            Player? ronTarget,
            bool riichi,
            bool doubleRiichi,
            bool openRiichi,
            bool 一発,
            bool 嶺上,
            bool 海底,
            bool 河底,
            bool 天和,
            bool 地和,
            bool 人和,
            bool 槍槓,
            TileType[] doraTiles,
            TileType[] uraDoraTiles)
        {
            this.structure = structure;
            this.newTileInHand = newTileInHand;
            this.ownWind = ownWind;
            this.roundWind = roundWind;
            this.ronTarget = ronTarget;
            Yakus = new Dictionary<役, int>();
            役満 = new Dictionary<役, int>();
            面前 = structure.melds.Length == 0 || structure.melds.All(_ => _.暗槓);
            this.doraTiles = doraTiles ?? System.Array.Empty<TileType>();
            this.uraDoraTiles = (riichi ? uraDoraTiles : null) ?? System.Array.Empty<TileType>();

            if (面前 && 自摸)
            {
                Yakus.Add(役.面前自摸, 1);
            }

            if (doubleRiichi)
            {
                Yakus.Add(役.ダブル立直, 2);
            }
            else if (riichi)
            {
                Yakus.Add(役.立直, 1);
            }
            if (openRiichi)
            {
                // 立直していない状態でオープンリーチに放銃すると役満払い
                if (ronTarget != null
                    && !ronTarget.Riichi)
                {
                    役満.Add(役.オープン立直, 1);
                }
                else
                {
                    Yakus.Add(役.オープン立直, 1);
                }
            }

            if (一発)
            {
                Yakus.Add(役.一発, 1);
            }

            if (嶺上)
            {
                Yakus.Add(役.嶺上開花, 1);
            }

            if (海底)
            {
                Yakus.Add(役.海底撈月, 1);
            }

            if (河底)
            {
                Yakus.Add(役.河底撈魚, 1);
            }
            if (槍槓)
            {
                Yakus.Add(役.槍槓, 1);
            }
            if (天和)
            {
                役満.Add(役.天和, 1);
            }
            if (地和)
            {
                役満.Add(役.地和, 1);
            }
            if (人和)
            {
                役満.Add(役.人和, 1);
            }
            {
                var v = タンヤオ;
                if (v > 0)
                {
                    Yakus.Add(役.タンヤオ, v);
                }
            }
            {
                var v = 平和;
                if (v > 0)
                {
                    Yakus.Add(役.平和, v);
                }
            }
            {
                var v = 白;
                if (v > 0)
                {
                    Yakus.Add(役.白, v);
                }
            }
            {
                var v = 發;
                if (v > 0)
                {
                    Yakus.Add(役.發, v);
                }
            }
            {
                var v = 中;
                if (v > 0)
                {
                    Yakus.Add(役.中, v);
                }
            }
            {
                var v = 場風牌;
                if (v > 0)
                {
                    Yakus.Add(役.場風, v);
                }
            }
            {
                var v = 自風牌;
                if (v > 0)
                {
                    Yakus.Add(役.自風, v);
                }
            }
            {
                var v = 七対子;
                if (v > 0)
                {
                    Yakus.Add(役.七対子, v);
                }
            }
            {
                var v = 対々和;
                if (v > 0)
                {
                    Yakus.Add(役.対々和, v);
                }
            }
            {
                var v = 三暗刻;
                if (v > 0)
                {
                    Yakus.Add(役.三暗刻, v);
                }
            }
            {
                var v = 三色同順;
                if (v > 0)
                {
                    Yakus.Add(役.三色同順, v);
                }
            }
            {
                var v = 三色同刻;
                if (v > 0)
                {
                    Yakus.Add(役.三色同刻, v);
                }
            }
            {
                var v = 混老頭;
                if (v > 0)
                {
                    Yakus.Add(役.混老頭, v);
                }
            }
            {
                var v = 一気通貫;
                if (v > 0)
                {
                    Yakus.Add(役.一気通貫, v);
                }
            }
            {
                var v = 小三元;
                if (v > 0)
                {
                    Yakus.Add(役.小三元, v);
                }
            }
            {
                var v = 三槓子;
                if (v > 0)
                {
                    Yakus.Add(役.三槓子, v);
                }
            }
            {
                var v = 純チャン;
                if (v > 0)
                {
                    Yakus.Add(役.純チャン, v);
                }
            }
            if (!Yakus.ContainsKey(役.純チャン)
                && !Yakus.ContainsKey(役.混老頭))
            {
                var v = チャンタ;
                if (v > 0)
                {
                    Yakus.Add(役.チャンタ, v);
                }
            }
            {
                var v = 二盃口;
                if (v > 0)
                {
                    Yakus.Add(役.二盃口, v);
                }
            }
            if (!Yakus.ContainsKey(役.二盃口))
            {
                var v = 一盃口;
                if (v > 0)
                {
                    Yakus.Add(役.一盃口, v);
                }
            }

            {
                var v = 清一色;
                if (v > 0)
                {
                    Yakus.Add(役.清一色, v);
                }
            }
            if (!Yakus.ContainsKey(役.清一色))
            {
                var v = 混一色;
                if (v > 0)
                {
                    Yakus.Add(役.混一色, v);
                }
            }

            if (緑一色)
            {
                役満.Add(役.緑一色, 1);
            }
            if (大三元)
            {
                役満.Add(役.大三元, 1);
            }
            if (字一色)
            {
                役満.Add(役.字一色, 1);
            }
            if (国士無双)
            {
                役満.Add(役.国士無双, 1);
            }
            if (九蓮宝燈)
            {
                役満.Add(役.九蓮宝燈, 1);
            }
            if (四暗刻)
            {
                役満.Add(役.四暗刻, 1);
            }
            if (清老頭)
            {
                役満.Add(役.清老頭, 1);
            }
            if (四槓子)
            {
                役満.Add(役.四槓子, 1);
            }

            if (大四喜)
            {
                役満.Add(役.大四喜, 2);
            }
            else if (小四喜)
            {
                役満.Add(役.小四喜, 1);
            }
        }

        readonly public int Fu
        {
            get
            {
                int fu;
                if (Yakus.ContainsKey(役.七対子))
                {
                    // 七対子は固定
                    fu = 25;
                }
                else if (Yakus.ContainsKey(役.平和))
                {
                    // 平和は固定
                    if (自摸)
                    {
                        fu = 20;
                    }
                    else
                    {
                        fu = 30;
                    }
                }
                else if (食い平和 && !自摸)
                {
                    // 食い平和のロンあがりは固定
                    fu = 30;
                }
                else
                {
                    // 基本20符
                    fu = 20;

                    // 暗刻
                    foreach (var it in Sets)
                    {
                        if (it.刻子)
                        {
                            // ロン上がりだと明刻扱いとなる
                            if (it.first == newTileInHand
                                && !自摸)
                            {
                                if (it.first.么九牌())
                                {
                                    fu += 4;
                                }
                                else
                                {
                                    fu += 2;
                                }
                            }
                            else
                            {
                                if (it.first.么九牌())
                                {
                                    fu += 8;
                                }
                                else
                                {
                                    fu += 4;
                                }
                            }
                        }
                    }
                    // 副露
                    foreach (var it in Melds)
                    {
                        if (it.暗槓)
                        {
                            if (it.tileFroms[0].tile.type.么九牌())
                            {
                                fu += 32;
                            }
                            else
                            {
                                fu += 16;
                            }
                        }
                        else if (it.槓子)
                        {
                            // 明槓
                            if (it.tileFroms[0].tile.type.么九牌())
                            {
                                fu += 16;
                            }
                            else
                            {
                                fu += 8;
                            }
                        }
                        else if (!it.順子)
                        {
                            // 明刻
                            if (it.tileFroms[0].tile.type.么九牌())
                            {
                                fu += 4;
                            }
                            else
                            {
                                fu += 2;
                            }
                        }
                    }
                    foreach (var it in Pairs)
                    {
                        if (it == ownWind)
                        {
                            fu += 2;
                        }
                        else if (it == roundWind)
                        {
                            fu += 2;
                        }
                        else if (it.三元牌())
                        {
                            fu += 2;
                        }
                    }

                    // 待ち
                    {
                        bool 待ち符 = false;
                        // 単騎
                        待ち符 |= Pairs.Contains(newTileInHand);

                        if (newTileInHand.IsSuited())
                        {
                            foreach (var it in Sets)
                            {
                                if (it.順子)
                                {
                                    // 嵌張
                                    待ち符 |= it.second == newTileInHand;

                                    // 辺張待ち
                                    if (it.first.Number() == 1
                                        && newTileInHand.Number() == 3)
                                    {
                                        待ち符 = true;
                                    }
                                    if (it.first.Number() == 7
                                        && newTileInHand.Number() == 7)
                                    {
                                        待ち符 = true;
                                    }
                                }
                            }
                        }
                        if (待ち符)
                        {
                            fu += 2;
                        }
                    }

                    if (面前 && !自摸)
                    {
                        fu += 10;
                    }
                    if (自摸)
                    {
                        fu += 2;
                    }

                    // 切り上げ
                    var fraction = fu % 10;
                    if (fraction > 0)
                    {
                        fu += 10 - fraction;
                    }
                }

                return fu;
            }
        }

        readonly public int Dora
        {
            get
            {
                if (doraTiles.Length == 0)
                {
                    return 0;
                }
                var allUsedTiles = AllUsedTiles.ToArray();
                return doraTiles.Sum(_ => allUsedTiles.Count(x => x == _));
            }
        }

        readonly public int UraDora
        {
            get
            {
                if (uraDoraTiles.Length == 0)
                {
                    return 0;
                }
                var allUsedTiles = AllUsedTiles.ToArray();
                return uraDoraTiles.Sum(_ => allUsedTiles.Count(x => x == _));
            }
        }

        readonly public (ScoreType? type, int score) 基本点(Rules.HandCap handCap)
        {
            int maxYakumanCount;
            switch(handCap)
            {
                case Rules.HandCap.役満:
                    maxYakumanCount = 1;
                    break;
                case Rules.HandCap.ダブル役満:
                    maxYakumanCount = 2;
                    break;
                case Rules.HandCap.トリプル役満:
                    maxYakumanCount = 3;
                    break;
                default:
                    throw new System.ArgumentException(handCap.ToString());
            }

            var yakumanCount = Mathf.Min(役満.Values.Sum(), maxYakumanCount);
            if (yakumanCount == 1)
            {
                return (ScoreType.役満, 8000);
            }
            else if (yakumanCount == 2)
            {
                return (ScoreType.ダブル役満, 16000);
            }
            else if (yakumanCount > 2)
            {
                return (ScoreType.トリプル役満, 24000);
            }

            var han = Han;

            if (han == 5)
            {
                return (ScoreType.満貫, 2000);
            }
            if (han >= 6 && han <= 7)
            {
                return (ScoreType.跳満, 3000);
            }
            if (han >= 8 && han <= 10)
            {
                return (ScoreType.倍満, 4000);
            }
            if (han >= 11 && han <= 12)
            {
                return (ScoreType.三倍満, 6000);
            }
            if (han >= 13)
            {
                return (ScoreType.数え役満, 8000);
            }

            var fu = Fu;

            var value = fu * (1 << (han + 2));
            if (value < 2000)
            {
                return (null, value);
            }
            else
            {
                return (ScoreType.満貫, 2000);
            }
        }

        readonly public CompletedResult BuildResult(Player player)
        {
            return new CompletedResult(this, player);
        }

        readonly IEnumerable<TileType> 刻子槓子
        {
            get
            {
                foreach (var it in Sets)
                {
                    if (it.刻子)
                    {
                        yield return it.first;
                    }
                }
                foreach (var it in Melds)
                {
                    if (!it.順子)
                    {
                        yield return it.tileFroms[0].tile.type;
                    }
                }
            }
        }
        readonly IEnumerable<TileType> 順子
        {
            get
            {
                foreach (var it in Sets)
                {
                    if (it.順子)
                    {
                        yield return it.first;
                    }
                }
                foreach (var it in Melds)
                {
                    if (it.順子)
                    {
                        yield return it.tileFroms[0].tile.type;
                    }
                }
            }
        }

        readonly bool 食い平和
        {
            get
            {
                if (Melds.Any(_ => !_.順子))
                {
                    return false;
                }
                if (Pairs.Length > 1)
                {
                    return false;
                }
                foreach (var it in Sets)
                {
                    if (it.刻子)
                    {
                        return false;
                    }
                }

                foreach (var it in AllUsedTiles)
                {
                    if (it.三元牌())
                    {
                        return false;
                    }
                    if (it == ownWind)
                    {
                        return false;
                    }
                    if (it == roundWind)
                    {
                        return false;
                    }
                }

                foreach (var it in Sets)
                {
                    if (it.first == newTileInHand
                        && it.first.Number() != 7)
                    {
                        return true;
                    }
                    if (it.third == newTileInHand
                        && it.first.Number() != 1)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        readonly int 平和
        {
            get
            {
                if (Melds.Length > 0)
                {
                    return 0;
                }
                if (!食い平和)
                {
                    return 0;
                }
                return 1;
            }
        }
        readonly int タンヤオ
        {
            get
            {
                foreach (var it in AllUsedTiles)
                {
                    if (!it.IsSuited())
                    {
                        return 0;
                    }
                    var number = it.Number();
                    if (number == 1)
                    {
                        return 0;
                    }
                    if (number == 9)
                    {
                        return 0;
                    }
                }
                return 1;
            }
        }

        readonly bool Has刻子槓子(TileType tile)
        {
            return 刻子槓子.Contains(tile);
        }

        readonly int 一盃口
        {
            get
            {
                if (!面前)
                {
                    return 0;
                }
                var dict = new IntDictionary<TileType>();
                foreach (var it in 順子)
                {
                    ++dict[it];
                }
                if (dict.Count(_ => _.Value == 2) == 1)
                {
                    return 1;
                }
                return 0;
            }
        }
        readonly int 二盃口
        {
            get
            {
                if (!面前)
                {
                    return 0;
                }
                var dict = new IntDictionary<TileType>();
                foreach (var it in 順子)
                {
                    ++dict[it];
                }
                if (dict.Count(_ => _.Value == 2) == 2)
                {
                    return 3;
                }
                return 0;
            }
        }

        readonly int 白 => Has刻子槓子(TileType.白) ? 1 : 0;
        readonly int 發 => Has刻子槓子(TileType.發) ? 1 : 0;
        readonly int 中 => Has刻子槓子(TileType.中) ? 1 : 0;
        readonly int 自風牌 => Has刻子槓子(ownWind) ? 1 : 0;
        readonly int 場風牌 => Has刻子槓子(roundWind) ? 1 : 0;

        readonly int 七対子 => (Pairs.Length == 7) ? 2 : 0;

        readonly int 対々和
        {
            get
            {
                if (刻子槓子.Count() == 4)
                {
                    return 2;
                }
                return 0;
            }
        }

        readonly bool N暗刻(int n)
        {
            var setCount = Sets.Count(_ => _.刻子) + Melds.Count(_ => _.暗槓);
            if (自摸)
            {
                return setCount == n;
            }

            if (Pairs.Contains(newTileInHand))
            {
                // 単騎待ち
                return (setCount == n);
            }
            else
            {
                // シャボ
                return (setCount == n + 1);
            }
        }

        readonly int 三暗刻 => N暗刻(3) ? 2 : 0;

        readonly int 三色同刻
        {
            get
            {
                var counters = new HashSet<SuitType>[9];

                foreach (var it in 刻子槓子)
                {
                    if (it.IsSuited())
                    {
                        if (counters[it.Number() - 1] == null)
                        {
                            counters[it.Number() - 1] = new HashSet<SuitType>();
                        }
                        counters[it.Number() - 1].Add(it.Suit());
                    }
                }

                if (counters.Any(_ => _ != null && _.Count >= 3))
                {
                    return 2;
                }
                return 0;
            }
        }

        readonly int 三色同順
        {
            get
            {
                var counters = new HashSet<SuitType>[9];

                foreach (var it in 順子)
                {
                    if (counters[it.Number() - 1] == null)
                    {
                        counters[it.Number() - 1] = new HashSet<SuitType>();
                    }
                    counters[it.Number() - 1].Add(it.Suit());
                }

                if (counters.Any(_ => _ != null && _.Count >= 3))
                {
                    if (面前)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
                return 0;
            }
        }

        readonly int 混老頭
        {
            get
            {
                foreach (var it in AllUsedTiles)
                {
                    if (it.IsSuited())
                    {
                        var number = it.Number();
                        if (number > 1 && number < 9)
                        {
                            return 0;
                        }
                    }
                }
                return 2;
            }
        }

        readonly int 一気通貫
        {
            get
            {
                var count = new Dictionary<SuitType, HashSet<int>>();
                foreach (var it in 順子)
                {
                    if (!count.TryGetValue(it.Suit(), out var set))
                    {
                        set = new HashSet<int>();
                        count.Add(it.Suit(), set);
                    }
                    var n = it.Number();
                    if (n == 1 || n == 4 || n == 7)
                    {
                        set.Add(n);
                    }
                }
                if (count.Any(_ => _.Value.Count == 3))
                {
                    if (面前)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
                return 0;
            }
        }

        readonly int チャンタ
        {
            get
            {
                foreach (var it in Pairs)
                {
                    if (it.IsSuited())
                    {
                        if (it.Number() > 1
                            && it.Number() < 9)
                        {
                            return 0;
                        }
                    }
                }
                foreach (var it in 刻子槓子)
                {
                    if (it.IsSuited())
                    {
                        if (it.Number() > 1
                        && it.Number() < 9)
                        {
                            return 0;
                        }
                    }
                }

                foreach (var it in 順子)
                {
                    if (it.Number() > 1
                        && it.Number() < 7)
                    {
                        return 0;
                    }
                }
                bool containsSuited = false;
                bool containsNotSuited = false;

                foreach (var it in AllUsedTiles)
                {
                    if (it.IsSuited())
                    {
                        containsSuited = true;
                    }
                    else
                    {
                        containsNotSuited = true;
                    }
                }

                if (!containsSuited)
                {
                    return 0;
                }
                if (!containsNotSuited)
                {
                    return 0;
                }

                if (面前)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
        }

        readonly int 小三元
        {
            get
            {
                if (!Pairs.Contains(TileType.白)
                    && !Pairs.Contains(TileType.發)
                    && !Pairs.Contains(TileType.中))
                {
                    return 0;
                }

                var count = 0;
                if (Has刻子槓子(TileType.白))
                {
                    ++count;
                }
                if (Has刻子槓子(TileType.發))
                {
                    ++count;
                }
                if (Has刻子槓子(TileType.中))
                {
                    ++count;
                }
                if (count == 2)
                {
                    return 2;
                }
                return 0;
            }
        }

        readonly int 三槓子
        {
            get
            {
                if (Melds.Count(_ => _.槓子) == 3)
                {
                    return 2;
                }
                return 0;
            }
        }

        readonly int 混一色
        {
            get
            {
                bool contains字牌 = false;
                SuitType? suit = null;
                foreach (var it in AllUsedTiles)
                {
                    if (it.IsSuited())
                    {
                        if (suit.HasValue)
                        {
                            if (suit.Value != it.Suit())
                            {
                                return 0;
                            }
                        }
                        else
                        {
                            suit = it.Suit();
                        }
                    }
                    else
                    {
                        contains字牌 = true;
                    }
                }
                if (!contains字牌)
                {
                    return 0;
                }
                if (面前)
                {
                    return 3;
                }
                return 2;
            }
        }

        readonly int 純チャン
        {
            get
            {
                foreach (var it in 順子)
                {
                    if (it.Number() > 1
                        && it.Number() < 7)
                    {
                        return 0;
                    }
                }
                foreach (var it in 刻子槓子.Concat(Pairs).Concat(IsolatedTiles))
                {
                    if (it.IsSuited())
                    {
                        if (it.Number() > 1
                            && it.Number() < 9)
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (面前)
                {
                    return 3;
                }
                return 2;
            }
        }

        readonly int 清一色
        {
            get
            {
                SuitType? suit = null;
                foreach (var it in AllUsedTiles)
                {
                    if (it.IsSuited())
                    {
                        if (suit.HasValue)
                        {
                            if (suit.Value != it.Suit())
                            {
                                return 0;
                            }
                        }
                        else
                        {
                            suit = it.Suit();
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
                if (面前)
                {
                    return 6;
                }
                return 5;
            }
        }

        static readonly TileType[] 緑一色牌 = new[] { TileType.S2, TileType.S3, TileType.S4, TileType.S6, TileType.S8, TileType.發 };

        readonly bool 緑一色
        {
            get
            {
                foreach (var it in AllUsedTiles)
                {
                    if (!緑一色牌.Contains(it))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        readonly bool 大三元
        {
            get
            {
                var tiles = 刻子槓子.ToArray();
                if (!tiles.Contains(TileType.白))
                {
                    return false;
                }
                if (!tiles.Contains(TileType.發))
                {
                    return false;
                }
                if (!tiles.Contains(TileType.中))
                {
                    return false;
                }
                return true;
            }
        }

        readonly bool 小四喜
        {
            get
            {
                var requires = new List<TileType> { TileType.東, TileType.西, TileType.南, TileType.北 };
                foreach (var it in 刻子槓子)
                {
                    requires.Remove(it);
                }
                if (requires.Count == 1)
                {
                    if (Pairs[0] == requires[0])
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        readonly bool 大四喜
        {
            get
            {
                var requires = new List<TileType> { TileType.東, TileType.西, TileType.南, TileType.北 };
                foreach (var it in 刻子槓子)
                {
                    requires.Remove(it);
                }
                return requires.Count == 0;
            }
        }

        readonly bool 字一色 => AllUsedTiles.All(_ => _.字牌());

        readonly bool 九蓮宝燈
        {
            get
            {
                // 暗槓も認められない
                if (Melds.Length > 0)
                {
                    return false;
                }

                var tiles = AllUsedTiles.ToList();
                var first = tiles[0];
                if (!first.IsSuited())
                {
                    return false;
                }
                var suit = first.Suit();
                tiles.Remove(TileTypeUtil.Get(suit, 1));
                tiles.Remove(TileTypeUtil.Get(suit, 1));
                tiles.Remove(TileTypeUtil.Get(suit, 1));
                tiles.Remove(TileTypeUtil.Get(suit, 2));
                tiles.Remove(TileTypeUtil.Get(suit, 3));
                tiles.Remove(TileTypeUtil.Get(suit, 4));
                tiles.Remove(TileTypeUtil.Get(suit, 5));
                tiles.Remove(TileTypeUtil.Get(suit, 6));
                tiles.Remove(TileTypeUtil.Get(suit, 7));
                tiles.Remove(TileTypeUtil.Get(suit, 8));
                tiles.Remove(TileTypeUtil.Get(suit, 9));
                tiles.Remove(TileTypeUtil.Get(suit, 9));
                tiles.Remove(TileTypeUtil.Get(suit, 9));
                if (tiles.Count > 1)
                {
                    return false;
                }
                return tiles[0].Suit() == suit;
            }
        }

        readonly bool 四暗刻 => N暗刻(4);
        readonly bool 清老頭 => AllUsedTiles.All(_ => _.IsSuited() && (_.Number() == 1 || _.Number() == 9));
        readonly bool 四槓子 => Melds.Count(_ => _.槓子) == 4;

        readonly bool 国士無双
        {
            get
            {
                if (Melds.Length > 0)
                {
                    return false;
                }
                if (Sets.Length > 0)
                {
                    return false;
                }
                if (IsolatedTiles.Length != 12)
                {
                    return false;
                }
                if (Pairs.Length != 1)
                {
                    return false;
                }

                var requires = new List<TileType>
                    {
                        TileType.M1,
                        TileType.M9,
                        TileType.S1,
                        TileType.S9,
                        TileType.P1,
                        TileType.P9,
                        TileType.白,
                        TileType.發,
                        TileType.中,
                        TileType.東,
                        TileType.南,
                        TileType.西,
                        TileType.北,
                    };
                foreach (var it in AllUsedTiles)
                {
                    requires.Remove(it);
                }
                return (requires.Count == 0);
            }
        }

        static public AfterDraw? Execute(Dictionary<Player, CompletedHand> completedHands,
            out RoundResult roundResult,
            out Dictionary<Player, CompletedResult> playerResults)
        {
            var round = completedHands.First().Key.round;
            var game = round.game;

            if (completedHands.Count == 3
                && game.rule.tripleRon == Rules.TripleRon.流局)
            {
                playerResults = new Dictionary<Player, CompletedResult>();
                var result = game.AdvanceRoundBy子上がり(out var gameResult);
                roundResult = new RoundResult(gameResult, scoreDiffs: round.players.ToDictionary(_ => _, _ => 0));
                return result;
            }

            playerResults = completedHands.ToDictionary(_ => _.Key, _ => _.Value.BuildResult(_.Key));

            var scoreDiffs = new Dictionary<Player, int>();
            foreach (var it in round.players)
            {
                scoreDiffs[it] = -it.scoreOwner.score;
            }

            foreach (var it in playerResults)
            {
                foreach (var pair in it.Value.scoreDiffs)
                {
                    pair.Key.scoreOwner.score += pair.Value;
                }
            }

            if (completedHands.Count == 1)
            {
                foreach (var it in completedHands.Keys)
                {
                    it.scoreOwner.score += game.riichiScore;
                }
            }
            else
            {
                // ダブロンのリーチ棒回収 : ロンされたプレイヤーから順番が近いほうをがリーチ棒を回収する
                var ronTarget = completedHands.First().Value.ronTarget!;
                var getter = completedHands.Keys
                    .OrderBy(_ => (_.index - ronTarget.index + _.round.players.Length) % _.round.players.Length)
                    .First();
                getter.scoreOwner.score += game.riichiScore;
            }
            game.riichiScore = 0;

            foreach (var it in round.players)
            {
                scoreDiffs[it] += it.scoreOwner.score;
            }

            if (completedHands.ContainsKey(round.Dealer))
            {
                var result = game.AdvanceRoundBy親上がり(out var gameResult);
                roundResult = new RoundResult(gameResult, scoreDiffs);
                return result;
            }
            else
            {
                var result = game.AdvanceRoundBy子上がり(out var gameResult);
                roundResult = new RoundResult(gameResult, scoreDiffs);
                return result;
            }
        }
    }
}