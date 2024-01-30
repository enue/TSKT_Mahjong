using System.Collections;
using System.Collections.Generic;
using TSKT.Mahjongs.Rules;
#nullable enable

namespace TSKT.Mahjongs
{
    [System.Serializable]
    public class RuleSetting
    {
        public PaymentRule payment = new();
        public EndRule end = new();

        public int initialScore = 25000;
        public RedTile redTile = RedTile.赤ドラ3;
        public TripleRon tripleRon = TripleRon.有効;
        public HandCap handCap = HandCap.トリプル役満;
        public 四家立直 四家立直 = 四家立直.流局;
        public 喰い替え 喰い替え = 喰い替え.なし;
        public OpenRiichi openRiichi = OpenRiichi.あり;
        public 明槓槓ドラ 明槓槓ドラ = 明槓槓ドラ.打牌後;
        public 四槓流れ 四槓流れ = 四槓流れ.流局;
    }
}
