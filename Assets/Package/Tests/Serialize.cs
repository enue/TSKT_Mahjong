using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using TSKT.Mahjongs;
using System.Linq;

namespace TSKT.Tests.Mahjongs
{
    public class Serialzie
    {
        [Test]
        public void ToJson()
        {
            var controller = Game.Create(0, new RuleSetting());
            var serializable = controller.SerializeGame();
            Debug.Log(JsonUtility.ToJson(serializable));
        }
    }
}
