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
    public enum HandCap
    {
        役満,
        ダブル役満,
        トリプル役満,
    }
    public enum 四家立直
    {
        続行,
        流局,
    }
    public enum 喰い替え
    {
        なし,
        あり,
    }
}
