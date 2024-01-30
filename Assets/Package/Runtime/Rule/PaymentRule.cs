using System.Collections;
using System.Collections.Generic;
#nullable enable

namespace TSKT.Mahjongs.Rules
{
    [System.Serializable]
    public class PaymentRule
    {
        public int 返し = 30000;
        public int[] ウマ = new[] { 20, 10, -10, -20 };
    }
}
