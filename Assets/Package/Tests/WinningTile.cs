using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using TSKT.Mahjongs;
using System.Linq;

namespace TSKT.Tests.Mahjongs
{
    public class WinningTile
    {
        [Test]
        [TestCase(new[] { TileType.M6 },
            TileType.M1, TileType.M1, TileType.M1,
            TileType.M3, TileType.M3, TileType.M3,
            TileType.P4, TileType.P4, TileType.P4,
            TileType.P5, TileType.P5, TileType.P5,
            TileType.M6)]
        [TestCase(new[] {TileType.P1, TileType.P9, TileType.M1,
            TileType.M9, TileType.S1, TileType.S9,
            TileType.白, TileType.發, TileType.中,
            TileType.東, TileType.西, TileType.南,
            TileType.北 },
            TileType.P1, TileType.P9, TileType.M1,
            TileType.M9, TileType.S1, TileType.S9,
            TileType.白, TileType.發, TileType.中,
            TileType.東, TileType.西, TileType.南,
            TileType.北)]
        [TestCase(new[] { TileType.北 },
            TileType.P1, TileType.P9, TileType.M1,
            TileType.M9, TileType.S1, TileType.S9,
            TileType.白, TileType.發, TileType.中,
            TileType.東, TileType.西, TileType.南,
            TileType.南)]
        [TestCase(new[] { TileType.P5, TileType.P8 },
            TileType.M1, TileType.M2, TileType.M3,
            TileType.M4, TileType.M5, TileType.M6,
            TileType.P1, TileType.P2, TileType.P3,
            TileType.P5, TileType.P5,
            TileType.P6, TileType.P7)]
        [TestCase(new[] { TileType.P2 },
            TileType.M1, TileType.M2, TileType.M3,
            TileType.M4, TileType.M5, TileType.M6,
            TileType.P2, TileType.P3, TileType.P4,
            TileType.P5, TileType.P5, TileType.P5,
            TileType.P5)]

        public void GetWinningTiles(TileType[] expecteds, params TileType[] handTiles)
        {
            var round = Game.Create(0, new RuleSetting()).Round;

            var hand = round.players[0].hand;
            hand.tiles.Clear();
            hand.tiles.AddRange(RandomUtil.GenerateShuffledArray(handTiles.Select(_ => new Tile(0, _, red: false)).ToList()));
            Assert.AreEqual(
                expecteds.OrderBy(_ => _).ToArray(),
                hand.GetWinningTiles().OrderBy(_ => _).ToArray());
            Assert.IsTrue(hand.向聴数IsLessThanOrEqual(0));
            Assert.IsFalse(hand.向聴数IsLessThanOrEqual(-1));
        }
    }
}
