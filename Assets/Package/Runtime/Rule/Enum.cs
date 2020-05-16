using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs.Rules
{
    public enum RedTile
    {
        赤無し,
        赤ドラ3,
        赤ドラ4,
    }
    public enum LengthType
    {
        東風戦,
        半荘戦,
        一荘戦,
    }
    public enum TripleRon
    {
        有効,
        流局,
        // 頭ハネ
    }
    public enum 役満複合
    {
        なし,
        ダブル役満あり,
        トリプル役満あり,
    }
}
