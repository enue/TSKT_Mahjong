using System.Collections;
using System.Collections.Generic;
#nullable enable

namespace TSKT.Mahjongs.Rules
{
    public enum 赤牌
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
    public enum トリロン
    {
        有効,
        流局,
        // 頭ハネ
    }
    public enum 役満複合上限
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
    public enum オープンリーチ
    {
        なし,
        あり,
    }
    public enum 明槓槓ドラ
    {
        打牌後,
        即ノリ
    }
    public enum アガリ止め
    {
        なし,
        あり,
    }
    public enum 四槓流れ
    {
        流局,
        続行,
    }
}
