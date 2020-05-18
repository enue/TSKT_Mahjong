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
        public void GetWinningTiles()
        {
            var round = Game.Create(0, new RuleSetting()).Round;
            var game = round.game;

            var serializableGame = game.ToSerializable();
            Debug.Log(JsonUtility.ToJson(serializableGame));

            var serializableRound = round.ToSerializable();
            Debug.Log(JsonUtility.ToJson(serializableRound));
        }
    }
}
