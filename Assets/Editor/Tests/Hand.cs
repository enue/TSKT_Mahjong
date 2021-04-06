using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using TSKT.Mahjongs;
using System.Linq;

namespace TSKT.Tests.Mahjongs
{
    public class Hand
    {
        [Test]
        [TestCase(true, TileType.M1, TileType.M2, TileType.M3, TileType.M3)]
        [TestCase(false, TileType.M1, TileType.M2, TileType.M3, TileType.M6)]
        [TestCase(false, TileType.M2, TileType.M2, TileType.M2, TileType.M6)]
        [TestCase(true, TileType.M2, TileType.M2, TileType.M2, TileType.M2)]
        [TestCase(true, TileType.M2, TileType.M3, TileType.M4, TileType.M1)]
        [TestCase(true, TileType.M2, TileType.M3, TileType.M4, TileType.M4)]
        [TestCase(false, TileType.M2, TileType.M3, TileType.M4, TileType.M2)]
        [TestCase(false, TileType.M2, TileType.M3, TileType.M4, TileType.M3)]
        public void 喰い替え(bool expected, TileType tile1, TileType tile2, TileType tileFromOtherPlayer, TileType tileToDiscard)
        {
            var meld = new Meld((new Tile(0, tile1, false), 0),
                (new Tile(0, tile2, false), 0),
                (new Tile(0, tileFromOtherPlayer, false), 1));

            Assert.AreEqual(expected, meld.Is喰い替え(new Tile(0, tileToDiscard, false)));
        }
    }
}