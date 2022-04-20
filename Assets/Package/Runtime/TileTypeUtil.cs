using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT.Mahjongs
{
    public static class TileTypeUtil
    {
        public static TileType[] CreateSet(bool 花牌, bool 季節牌)
        {
            // 数牌、字牌 各四枚
            // 花牌、季節牌各1枚

            var result = new List<TileType>();
            foreach (TileType it in System.Enum.GetValues(typeof(TileType)))
            {
                if (it.季節牌())
                {
                    if (季節牌)
                    {
                        result.Add(it);
                    }
                }
                else if (it.花牌())
                {
                    if (花牌)
                    {
                        result.Add(it);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        result.Add(it);
                    }
                }
            }

            return result.ToArray();
        }

        public static TileType GetDora(TileType indicatorTile)
        {
            if (indicatorTile.IsSuited())
            {
                var number = indicatorTile.Number() % 9 + 1;
                return Get(indicatorTile.Suit(), number);
            }
            if (indicatorTile == TileType.白)
            {
                return TileType.發;
            }
            if (indicatorTile == TileType.發)
            {
                return TileType.中;
            }
            if (indicatorTile == TileType.中)
            {
                return TileType.白;
            }
            if (indicatorTile == TileType.東)
            {
                return TileType.南;
            }
            if (indicatorTile == TileType.南)
            {
                return TileType.西;
            }
            if (indicatorTile == TileType.西)
            {
                return TileType.北;
            }
            if (indicatorTile == TileType.北)
            {
                return TileType.東;
            }

            throw new System.ArgumentException(indicatorTile.ToString());
        }

        public static TileType Get(SuitType suit, int number)
        {
            var value = (int)suit * 9 + number - 1;
            return (TileType)value;
        }

        public static bool IsPin(this TileType tile)
        {
            return tile == TileType.P1
                || (tile == TileType.P2)
                || (tile == TileType.P3)
                || (tile == TileType.P4)
                || (tile == TileType.P5)
                || (tile == TileType.P6)
                || (tile == TileType.P7)
                || (tile == TileType.P8)
                || (tile == TileType.P9);
        }
        public static bool IsSou(this TileType tile)
        {
            return tile == TileType.S1
                || (tile == TileType.S2)
                || (tile == TileType.S3)
                || (tile == TileType.S4)
                || (tile == TileType.S5)
                || (tile == TileType.S6)
                || (tile == TileType.S7)
                || (tile == TileType.S8)
                || (tile == TileType.S9);
        }

        public static bool IsMan(this TileType tile)
        {
            return tile == TileType.M1
                || (tile == TileType.M2)
                || (tile == TileType.M3)
                || (tile == TileType.M4)
                || (tile == TileType.M5)
                || (tile == TileType.M6)
                || (tile == TileType.M7)
                || (tile == TileType.M8)
                || (tile == TileType.M9);
        }

        public static bool IsSuited(this TileType tile)
        {
            return tile.IsMan() || tile.IsPin() || tile.IsSou();
        }

        public static bool 字牌(this TileType tile)
        {
            return !tile.IsSuited();
        }

        public static SuitType Suit(this TileType tile)
        {
            if (tile.IsMan())
            {
                return SuitType.萬子;
            }
            if (tile.IsPin())
            {
                return SuitType.筒子;
            }
            if (tile.IsSou())
            {
                return SuitType.索子;
            }

            throw new System.ArgumentException(tile.ToString());
        }

        public static int Number(this TileType tile)
        {
            if (tile.IsMan())
            {
                return (int)tile - (int)TileType.M1 + 1;
            }
            if (tile.IsPin())
            {
                return (int)tile - (int)TileType.P1 + 1;
            }
            if (tile.IsSou())
            {
                return (int)tile - (int)TileType.S1 + 1;
            }

            throw new System.ArgumentException(tile.ToString());
        }

        public static bool 季節牌(this TileType tile)
        {
            switch (tile)
            {
                case TileType.春:
                case TileType.夏:
                case TileType.秋:
                case TileType.冬:
                    return true;
                default:
                    return false;
            }
        }

        public static bool 花牌(this TileType tile)
        {
            switch (tile)
            {
                case TileType.梅:
                case TileType.竹:
                case TileType.蘭:
                case TileType.菊:
                    return true;
                default:
                    return false;
            }
        }

        public static bool 么九牌(this TileType tile)
        {
            switch (tile)
            {
                case TileType.M1:
                case TileType.M9:
                case TileType.S1:
                case TileType.S9:
                case TileType.P1:
                case TileType.P9:
                case TileType.白:
                case TileType.發:
                case TileType.中:
                case TileType.東:
                case TileType.西:
                case TileType.南:
                case TileType.北:
                    return true;
                default:
                    return false;
            }
        }
        public static bool 三元牌(this TileType tile)
        {
            switch (tile)
            {
                case TileType.白:
                case TileType.發:
                case TileType.中:
                    return true;
                default:
                    return false;
            }
        }
        public static bool 風牌(this TileType tile)
        {
            switch (tile)
            {
                case TileType.東:
                case TileType.南:
                case TileType.西:
                case TileType.北:
                    return true;
                default:
                    return false;
            }
        }

        public static string GetDisplayName(this TileType tile)
        {
            switch (tile)
            {
                case TileType.M1: return "一萬";
                case TileType.M2: return "二萬";
                case TileType.M3: return "三萬";
                case TileType.M4: return "四萬";
                case TileType.M5: return "五萬";
                case TileType.M6: return "六萬";
                case TileType.M7: return "七萬";
                case TileType.M8: return "八萬";
                case TileType.M9: return "九萬";
                case TileType.P1: return "一筒";
                case TileType.P2: return "二筒";
                case TileType.P3: return "三筒";
                case TileType.P4: return "四筒";
                case TileType.P5: return "五筒";
                case TileType.P6: return "六筒";
                case TileType.P7: return "七筒";
                case TileType.P8: return "八筒";
                case TileType.P9: return "九筒";
                case TileType.S1: return "一索";
                case TileType.S2: return "二索";
                case TileType.S3: return "三索";
                case TileType.S4: return "四索";
                case TileType.S5: return "五索";
                case TileType.S6: return "六索";
                case TileType.S7: return "七索";
                case TileType.S8: return "八索";
                case TileType.S9: return "九索";
                case TileType.東: return "東";
                case TileType.南: return "南";
                case TileType.西: return "西";
                case TileType.北: return "北";
                case TileType.白: return "白";
                case TileType.發: return "發";
                case TileType.中: return "中";
                case TileType.春: return "春";
                case TileType.夏: return "夏";
                case TileType.秋: return "秋";
                case TileType.冬: return "冬";

                case TileType.梅: return "梅";
                case TileType.蘭: return "蘭";
                case TileType.菊: return "菊";
                case TileType.竹: return "竹";
                default:
                    throw new System.ArgumentException(tile.ToString());
            }
        }
        public static string GetEmoji(this TileType tile)
        {
            return tile switch
            {
                TileType.M1 => "🀇",
                TileType.M2 => "🀈",
                TileType.M3 => "🀉",
                TileType.M4 => "🀊",
                TileType.M5 => "🀋",
                TileType.M6 => "🀌",
                TileType.M7 => "🀍",
                TileType.M8 => "🀎",
                TileType.M9 => "🀏",
                TileType.P1 => "🀙",
                TileType.P2 => "🀚",
                TileType.P3 => "🀛",
                TileType.P4 => "🀜",
                TileType.P5 => "🀝",
                TileType.P6 => "🀞",
                TileType.P7 => "🀟",
                TileType.P8 => "🀠",
                TileType.P9 => "🀡",
                TileType.S1 => "🀐",
                TileType.S2 => "🀑",
                TileType.S3 => "🀒",
                TileType.S4 => "🀓",
                TileType.S5 => "🀔",
                TileType.S6 => "🀕",
                TileType.S7 => "🀖",
                TileType.S8 => "🀗",
                TileType.S9 => "🀘",
                TileType.東 => "🀀",
                TileType.南 => "🀁",
                TileType.西 => "🀂",
                TileType.北 => "🀃",
                TileType.白 => "🀆",
                TileType.發 => "🀅",
                TileType.中 => "🀄",
                TileType.春 => "🀦",
                TileType.夏 => "🀧",
                TileType.秋 => "🀨",
                TileType.冬 => "🀩",

                TileType.梅 => "🀢",
                TileType.蘭 => "🀣",
                TileType.菊 => "🀥",
                TileType.竹 => "🀤",
                _ => throw new System.ArgumentException(tile.ToString())
            };
        }
    }

}
