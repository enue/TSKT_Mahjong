using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT.Mahjongs.Rules;

namespace TSKT.Mahjongs
{
    public class RuleSetting
    {
        public PaymentRule payment = new PaymentRule(30000, 20, 10, -10, -20);
        public EndRule end = new EndRule();

        public int initialScore = 25000;
        public RedTile redTile = RedTile.赤ドラ3;
        public TripleRon tripleRon = TripleRon.有効;
        public HandCap handCap = HandCap.トリプル役満;
        public 四家立直 四家立直 = 四家立直.流局;
    }
}
