using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT.Mahjongs.Rules
{
    public readonly struct PaymentRule
    {
        readonly public int 返し;
        readonly public int[] ウマ;

        public PaymentRule(int 返し, params int[] ウマ)
        {
            this.返し = 返し;
            this.ウマ = ウマ;
        }
    }
}
