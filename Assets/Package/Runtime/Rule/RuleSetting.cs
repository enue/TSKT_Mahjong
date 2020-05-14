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

        public int initialPoint = 25000;
        public RedTile redTile = RedTile.赤ドラ3;
    }
}
