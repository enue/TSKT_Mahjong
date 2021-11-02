using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT.Mahjongs
{
    public enum 役
    {
        門前清自摸和,
        立直,
        一発,
        タンヤオ,
        平和,
        一盃口,
        白,
        發,
        中,
        場風,
        自風,
        槍槓,
        嶺上開花,
        海底撈月,
        河底撈魚,

        ダブル立直,
        オープン立直,
        七対子,
        対々和,
        三暗刻,
        三色同刻,
        三色同順,
        混老頭,
        一気通貫,
        チャンタ,
        小三元,
        三槓子,

        混一色,
        純チャン,
        二盃口,

        流し満貫,

        清一色,

        天和,
        地和,
        人和,
        緑一色,
        大三元,
        小四喜,
        字一色,
        国士無双,
        九蓮宝燈,
        四暗刻,
        清老頭,
        四槓子,

        四暗刻単騎,
        大四喜,
        純正九蓮宝燈,
        国士無双十三面待ち,
    }

    public enum ExhausiveDrawType
    {
        ノーテン,
        テンパイ,
        流し満貫,
    }

    public enum ScoreType
    {
        満貫,
        跳満,
        倍満,
        三倍満,
        役満,
        数え役満,
        ダブル役満,
        トリプル役満,
    }

    public enum RelativePlayer
    {
        自家,
        下家,
        対面,
        上家,
    }

    public enum PlayerIndex
    {
        Index0 = 0,
        Index1,
        Index2,
        Index3,
    }

    public static class RelativePlayerUtil
    {
        static public RelativePlayer GetByPlayerIndex(PlayerIndex fromIndex, PlayerIndex targetIndex)
        {
            var diff = (targetIndex - fromIndex + 4) % 4;
            if (diff == 0)
            {
                return RelativePlayer.自家;
            }
            if (diff == 1)
            {
                return RelativePlayer.下家;
            }
            if (diff == 2)
            {
                return RelativePlayer.対面;
            }
            if (diff == 3)
            {
                return RelativePlayer.上家;
            }
            throw new System.ArgumentException();

        }
    }
}
