#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs.Rounds
{
    public readonly struct 和了
    {
        public readonly Hands.Structure structure;
        public readonly TileType[] 浮き牌 => structure.浮き牌;
        public readonly Hands.Structure.面子[] 面子 => structure.面子s;
        public readonly TileType[] 対子 => structure.対子;
        public readonly 副露[] 副露 => structure.副露;
        public readonly int 赤牌 => structure.赤牌;

        public readonly Player? ronTarget;

        readonly TileType 和了牌;
        readonly TileType 自風;
        readonly TileType 場風;
        public readonly Dictionary<役, int> 役;
        public readonly Dictionary<役, int> 役満;

        public readonly bool 役無し => 役.Count == 0 && 役満.Count == 0;
        public readonly int 翻 => 役.Values.Sum() + ドラ + 裏ドラ + 赤牌;
        public readonly bool 面前;
        public readonly bool 自摸 => ronTarget == null;
        public readonly TileType[] doraTiles;
        public readonly TileType[] uraDoraTiles;
        readonly IEnumerable<TileType> AllUsedTiles => structure.AllUsedTiles;

        public 和了(Hands.Structure structure, TileType 和了牌, TileType ownWind, TileType roundWind,
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
            this.和了牌 = 和了牌;
            this.自風 = ownWind;
            this.場風 = roundWind;
            this.ronTarget = ronTarget;
            役 = new Dictionary<役, int>();
            役満 = new Dictionary<役, int>();
            面前 = structure.副露.Length == 0 || structure.副露.All(_ => _.暗槓);
            this.doraTiles = doraTiles ?? System.Array.Empty<TileType>();
            this.uraDoraTiles = (riichi ? uraDoraTiles : null) ?? System.Array.Empty<TileType>();

            if (面前 && 自摸)
            {
                役.Add(Mahjongs.役.門前清自摸和, 1);
            }

            if (doubleRiichi)
            {
                役.Add(Mahjongs.役.ダブル立直, 2);
            }
            else if (riichi)
            {
                役.Add(Mahjongs.役.立直, 1);
            }
            if (openRiichi)
            {
                // 立直していない状態でオープンリーチに放銃すると役満払い
                if (ronTarget != null
                    && !ronTarget.リーチ)
                {
                    役満.Add(Mahjongs.役.オープン立直, 1);
                }
                else
                {
                    役.Add(Mahjongs.役.オープン立直, 1);
                }
            }

            if (一発)
            {
                役.Add(Mahjongs.役.一発, 1);
            }

            if (嶺上)
            {
                役.Add(Mahjongs.役.嶺上開花, 1);
            }

            if (海底)
            {
                役.Add(Mahjongs.役.海底撈月, 1);
            }

            if (河底)
            {
                役.Add(Mahjongs.役.河底撈魚, 1);
            }
            if (槍槓)
            {
                役.Add(Mahjongs.役.槍槓, 1);
            }
            if (天和)
            {
                役満.Add(Mahjongs.役.天和, 1);
            }
            if (地和)
            {
                役満.Add(Mahjongs.役.地和, 1);
            }
            if (人和)
            {
                役満.Add(Mahjongs.役.人和, 1);
            }
            {
                var v = タンヤオ;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.タンヤオ, v);
                }
            }
            {
                var v = 平和;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.平和, v);
                }
            }
            {
                var v = 白;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.白, v);
                }
            }
            {
                var v = 發;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.發, v);
                }
            }
            {
                var v = 中;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.中, v);
                }
            }
            {
                var v = 場風牌;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.場風, v);
                }
            }
            {
                var v = 自風牌;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.自風, v);
                }
            }
            {
                var v = 七対子;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.七対子, v);
                }
            }
            {
                var v = 対々和;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.対々和, v);
                }
            }
            {
                var v = 三暗刻;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.三暗刻, v);
                }
            }
            {
                var v = 三色同順;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.三色同順, v);
                }
            }
            {
                var v = 三色同刻;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.三色同刻, v);
                }
            }
            {
                var v = 混老頭;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.混老頭, v);
                }
            }
            {
                var v = 一気通貫;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.一気通貫, v);
                }
            }
            {
                var v = 小三元;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.小三元, v);
                }
            }
            {
                var v = 三槓子;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.三槓子, v);
                }
            }
            {
                var v = 純チャン;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.純チャン, v);
                }
            }
            if (!役.ContainsKey(Mahjongs.役.純チャン)
                && !役.ContainsKey(Mahjongs.役.混老頭))
            {
                var v = チャンタ;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.チャンタ, v);
                }
            }
            {
                var v = 二盃口;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.二盃口, v);
                }
            }
            if (!役.ContainsKey(Mahjongs.役.二盃口))
            {
                var v = 一盃口;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.一盃口, v);
                }
            }

            {
                var v = 清一色;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.清一色, v);
                }
            }
            if (!役.ContainsKey(Mahjongs.役.清一色))
            {
                var v = 混一色;
                if (v > 0)
                {
                    役.Add(Mahjongs.役.混一色, v);
                }
            }

            if (緑一色)
            {
                役満.Add(Mahjongs.役.緑一色, 1);
            }
            if (大三元)
            {
                役満.Add(Mahjongs.役.大三元, 1);
            }
            if (字一色)
            {
                役満.Add(Mahjongs.役.字一色, 1);
            }
            if (国士無双)
            {
                役満.Add(Mahjongs.役.国士無双, 1);
            }
            if (九蓮宝燈)
            {
                役満.Add(Mahjongs.役.九蓮宝燈, 1);
            }
            if (四暗刻)
            {
                役満.Add(Mahjongs.役.四暗刻, 1);
            }
            if (清老頭)
            {
                役満.Add(Mahjongs.役.清老頭, 1);
            }
            if (四槓子)
            {
                役満.Add(Mahjongs.役.四槓子, 1);
            }

            if (大四喜)
            {
                役満.Add(Mahjongs.役.大四喜, 2);
            }
            else if (小四喜)
            {
                役満.Add(Mahjongs.役.小四喜, 1);
            }
        }

        readonly public int 符
        {
            get
            {
                int fu;
                if (役.ContainsKey(Mahjongs.役.七対子))
                {
                    // 七対子は固定
                    fu = 25;
                }
                else if (役.ContainsKey(Mahjongs.役.平和))
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
                    foreach (var it in 面子)
                    {
                        if (it.刻子)
                        {
                            // ロン上がりだと明刻扱いとなる
                            if (it.first == 和了牌
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
                    foreach (var it in 副露)
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
                    foreach (var it in 対子)
                    {
                        if (it == 自風)
                        {
                            fu += 2;
                        }
                        else if (it == 場風)
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
                        待ち符 |= 対子.Contains(和了牌);

                        if (和了牌.Is数牌())
                        {
                            foreach (var it in 面子)
                            {
                                if (it.順子)
                                {
                                    // 嵌張
                                    待ち符 |= it.second == 和了牌;

                                    // 辺張待ち
                                    if (it.first.Number() == 1
                                        && 和了牌.Number() == 3)
                                    {
                                        待ち符 = true;
                                    }
                                    if (it.first.Number() == 7
                                        && 和了牌.Number() == 7)
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

        readonly public int ドラ
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

        readonly public int 裏ドラ
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

        readonly public (ScoreType? type, int score) 基本点(Rules.役満複合上限 handCap)
        {
            int maxYakumanCount;
            switch(handCap)
            {
                case Rules.役満複合上限.役満:
                    maxYakumanCount = 1;
                    break;
                case Rules.役満複合上限.ダブル役満:
                    maxYakumanCount = 2;
                    break;
                case Rules.役満複合上限.トリプル役満:
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

            var han = 翻;

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

            var fu = 符;

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

        readonly public 和了Result BuildResult(Player player)
        {
            var result = new 和了Result(this, player);
            foreach(var it in player.局.game.completedHandModifiers)
            {
                it.Modify(ref result);
            }
            return result;
        }

        readonly IEnumerable<TileType> 刻子槓子
        {
            get
            {
                foreach (var it in 面子)
                {
                    if (it.刻子)
                    {
                        yield return it.first;
                    }
                }
                foreach (var it in 副露)
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
                foreach (var it in 面子)
                {
                    if (it.順子)
                    {
                        yield return it.first;
                    }
                }
                foreach (var it in 副露)
                {
                    if (it.順子)
                    {
                        yield return it.tileFroms[0].tile.type;
                    }
                }
            }
        }

        readonly bool 平和形(out bool 役, out bool 食い平和)
        {
            if (副露.Any(_ => !_.順子))
            {
                役 = false;
                食い平和 = false;
                return false;
            }
            var 鳴き = 副露.Any();

            if (対子.Length > 1)
            {
                役 = false;
                食い平和 = false;
                return false;
            }
            foreach (var it in 面子)
            {
                if (it.刻子)
                {
                    役 = false;
                    食い平和 = false;
                    return false;
                }
            }

            foreach (var it in AllUsedTiles)
            {
                if (it.三元牌())
                {
                    役 = false;
                    食い平和 = false;
                    return false;
                }
                if (it == 自風)
                {
                    役 = false;
                    食い平和 = false;
                    return false;
                }
                if (it == 場風)
                {
                    役 = false;
                    食い平和 = false;
                    return false;
                }
            }

            foreach (var it in 面子)
            {
                if (it.first == 和了牌
                    && it.first.Number() != 7)
                {
                    役 = !副露.Any();
                    食い平和 = 副露.Any();
                    return true;
                }
                if (it.third == 和了牌
                    && it.first.Number() != 1)
                {
                    役 = !副露.Any();
                    食い平和 = 副露.Any();
                    return true;
                }
            }
            役 = false;
            食い平和 = false;
            return false;
        }

        readonly bool 食い平和
        {
            get
            {
                平和形(out _, out var result);
                return result;
            }
        }
        readonly int 平和
        {
            get
            {
                平和形(out var yaku, out _);
                return yaku ? 1 : 0;
            }
        }
        readonly int タンヤオ
        {
            get
            {
                foreach (var it in AllUsedTiles)
                {
                    if (!it.Is数牌())
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
                var count = 順子
                    .GroupBy(_ => _)
                    .Select(_ => _.Count())
                    .Count(_ => _ == 2);
                if (count == 1)
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
                var count = 順子
                    .GroupBy(_ => _)
                    .Select(_ => _.Count())
                    .Count(_ => _ == 2);
                if (count == 2)
                {
                    return 3;
                }
                return 0;
            }
        }

        readonly int 白 => Has刻子槓子(TileType.白) ? 1 : 0;
        readonly int 發 => Has刻子槓子(TileType.發) ? 1 : 0;
        readonly int 中 => Has刻子槓子(TileType.中) ? 1 : 0;
        readonly int 自風牌 => Has刻子槓子(自風) ? 1 : 0;
        readonly int 場風牌 => Has刻子槓子(場風) ? 1 : 0;

        readonly int 七対子 => (対子.Length == 7) ? 2 : 0;

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
            var setCount = 面子.Count(_ => _.刻子) + 副露.Count(_ => _.暗槓);
            if (自摸)
            {
                return setCount == n;
            }

            if (対子.Contains(和了牌))
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
                    if (it.Is数牌())
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
                    if (it.Is数牌())
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
                foreach (var it in 対子)
                {
                    if (it.Is数牌())
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
                    if (it.Is数牌())
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
                    if (it.Is数牌())
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
                if (!対子.Contains(TileType.白)
                    && !対子.Contains(TileType.發)
                    && !対子.Contains(TileType.中))
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
                if (副露.Count(_ => _.槓子) == 3)
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
                    if (it.Is数牌())
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
                foreach (var it in 刻子槓子.Concat(対子).Concat(浮き牌))
                {
                    if (it.Is数牌())
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
                    if (it.Is数牌())
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
                    if (対子[0] == requires[0])
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

        readonly bool 字一色 => AllUsedTiles.All(_ => _.Is字牌());

        readonly bool 九蓮宝燈
        {
            get
            {
                // 暗槓も認められない
                if (副露.Length > 0)
                {
                    return false;
                }

                var tiles = AllUsedTiles.ToList();
                var first = tiles[0];
                if (!first.Is数牌())
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
        readonly bool 清老頭 => AllUsedTiles.All(_ => _.Is数牌() && (_.Number() == 1 || _.Number() == 9));
        readonly bool 四槓子 => 副露.Count(_ => _.槓子) == 4;

        readonly bool 国士無双
        {
            get
            {
                if (副露.Length > 0)
                {
                    return false;
                }
                if (面子.Length > 0)
                {
                    return false;
                }
                if (浮き牌.Length != 12)
                {
                    return false;
                }
                if (対子.Length != 1)
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

        public static CommandResult Execute(params (Player player, 和了 hand)[] 和了)
        {
            var round = 和了[0].player.局;
            var game = round.game;

            if (和了.Length == 3
                && game.rule.tripleRon == Rules.トリロン.流局)
            {
                var playerResults = new Dictionary<Player, 和了Result>();
                var result = game.Advance局By子上がり(out var gameResult);
                var roundResult = new 局Result(gameResult, scoreDiffs: round.players.ToDictionary(_ => _, _ => 0));
                return new CommandResult(result, roundResult, playerResults);
            }
            else
            {
                var playerResults = 和了.ToDictionary(_ => _.player, _ => _.hand.BuildResult(_.player));

                var scoreDiffs = new Dictionary<Player, int>();
                foreach (var it in round.players)
                {
                    scoreDiffs[it] = -it.Score;
                }

                foreach (var it in playerResults)
                {
                    foreach (var pair in it.Value.scoreDiffs)
                    {
                        pair.Key.Score += pair.Value;
                    }
                }

                if (和了.Length == 1)
                {
                    foreach (var (player, hand) in 和了)
                    {
                        player.Score += game.リーチ棒スコア;
                    }
                }
                else
                {
                    // ダブロンのリーチ棒回収 : ロンされたプレイヤーから順番が近いほうをがリーチ棒を回収する
                    var ronTarget = 和了[0].hand.ronTarget!;
                    var (player, hand) = 和了
                        .OrderBy(_ => (_.player.index - ronTarget.index + _.player.局.players.Length) % _.player.局.players.Length)
                        .First();
                    player.Score += game.リーチ棒スコア;
                }
                game.リーチ棒スコア = 0;

                foreach (var it in round.players)
                {
                    scoreDiffs[it] += it.Score;
                }

                if (和了.Select(_ => _.player).Contains(round.親))
                {
                    var result = game.Advance局By親上がり(out var gameResult);
                    var roundResult = new 局Result(gameResult, scoreDiffs);
                    return new CommandResult(result, roundResult, playerResults);
                }
                else
                {
                    var result = game.Advance局By子上がり(out var gameResult);
                    var roundResult = new 局Result(gameResult, scoreDiffs);
                    return new CommandResult(result, roundResult, playerResults);
                }
            }
        }
    }
}