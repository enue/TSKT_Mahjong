#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using TSKT.Mahjongs;
using TSKT.Mahjongs.Rounds;
using System.Linq;

namespace TSKT.Tests.Mahjongs
{
    public class Score
    {
        [Test]
        [TestCase(-1, TileType.東, TileType.東, TileType.東, TileType.南, TileType.南, TileType.南, TileType.西, TileType.西, TileType.西, TileType.北, TileType.北, TileType.北, TileType.白, TileType.白)]
        [TestCase(-1, TileType.M1, TileType.M2, TileType.M3, TileType.M2, TileType.M3, TileType.M4, TileType.M5, TileType.M6, TileType.M7, TileType.S1, TileType.S2, TileType.S3, TileType.白, TileType.白)]
        [TestCase(-1, TileType.M1, TileType.M1, TileType.M2, TileType.M2, TileType.M3, TileType.M3, TileType.S7, TileType.S7, TileType.S8, TileType.S8, TileType.S9, TileType.S9, TileType.M5, TileType.M5)]
        [TestCase(-1, TileType.M1, TileType.M1, TileType.M2, TileType.M2, TileType.M4, TileType.M4, TileType.S7, TileType.S7, TileType.S8, TileType.S8, TileType.S9, TileType.S9, TileType.M5, TileType.M5)]
        [TestCase(-1, TileType.東, TileType.南, TileType.西, TileType.北, TileType.白, TileType.發, TileType.中, TileType.S1, TileType.S9, TileType.M1, TileType.M9, TileType.P1, TileType.P9, TileType.P9)]
        [TestCase(-1, TileType.白, TileType.白, TileType.白, TileType.發, TileType.發, TileType.發, TileType.中, TileType.中, TileType.中, TileType.M2, TileType.M3, TileType.M4, TileType.P9, TileType.P9)]
        [TestCase(-1, TileType.白, TileType.白, TileType.白, TileType.發, TileType.發, TileType.發, TileType.中, TileType.中, TileType.M2, TileType.M3, TileType.M4, TileType.P7, TileType.P8, TileType.P9)]
        [TestCase(-1, TileType.M1, TileType.M2, TileType.M3, TileType.P1, TileType.P2, TileType.P3, TileType.S1, TileType.S2, TileType.S3, TileType.S1, TileType.S2, TileType.S3, TileType.白, TileType.白)]
        [TestCase(-1, TileType.M2, TileType.M2, TileType.M2, TileType.P2, TileType.P2, TileType.P2, TileType.S2, TileType.S2, TileType.S2, TileType.S1, TileType.S2, TileType.S3, TileType.白, TileType.白)]
        [TestCase(-1, TileType.M2, TileType.M3, TileType.M4, TileType.P1, TileType.P2, TileType.P3, TileType.S1, TileType.S2, TileType.S3, TileType.S5, TileType.S6, TileType.S7, TileType.北, TileType.北)]
        [TestCase(-1, TileType.M2, TileType.M3, TileType.M4, TileType.P4, TileType.P5, TileType.P6, TileType.S3, TileType.S4, TileType.S5, TileType.S5, TileType.S6, TileType.S7, TileType.S8, TileType.S8)]
        [TestCase(-1, TileType.M1, TileType.M2, TileType.M3, TileType.M4, TileType.M5, TileType.M6, TileType.M4, TileType.M5, TileType.M6, TileType.P7, TileType.P8, TileType.P9, TileType.M8, TileType.M8)]
        [TestCase(-1, TileType.M2, TileType.M2, TileType.白, TileType.白, TileType.白, TileType.發, TileType.發, TileType.發, TileType.中, TileType.中, TileType.中, TileType.M3, TileType.M3, TileType.M3)]
        public void シャンテン数(int expected, params TileType[] tiles)
        {
            var round = Game.Create(0, new RuleSetting()).ResetRound(tiles).Round;

            {
                var hand = round.players[0].hand;
                hand.tiles.Clear();
                hand.tiles.AddRange(RandomUtil.GenerateShuffledArray(tiles.Select(_ => new Tile(0, _, red: false)).ToList()));
                var solution = hand.Solve();
                Assert.AreEqual(expected, solution.向聴数);
                Assert.IsTrue(hand.向聴数IsLessThanOrEqual(expected));

                if (expected == -1)
                {
                    var completed = solution.ChoiceCompletedHand(newTileInHand: tiles[0], ownWind: TileType.東, roundWind: TileType.南,
                        ronTarget: null,
                        riichi: true,
                        doubleRiichi: false,
                        openRiichi: false,
                        一発: false,
                        嶺上: false,
                        海底: false,
                        河底: false,
                        天和: false,
                        地和: false,
                        人和: false,
                        doraTiles: new TileType[0],
                        uraDoraTiles: new TileType[0],
                        槍槓: false,
                        handCap: round.game.rule.handCap);
                    var player = round.players[(int)round.dealer + 1];
                    var r = CompletedHand.Execute((player, completed));
                    var result = r.completedResults[player];
                    Debug.Log(result.tsumoPenalty!.Value);
                    Debug.Log(result.displayScore?.han + "翻 " + result.displayScore?.fu + "符 " + result.scoreType);
                    Debug.Log(string.Join(", ", completed.Yakus.Keys.Concat(completed.役満.Keys)));
                    Assert.AreEqual(0, r.roundResult.scoreDiffs!.Values.Sum());
                }
            }

            // 上がりからひとつ抜いたらリーチ
            if (expected == -1)
            {
                var hand = round.players[0].hand;
                hand.tiles.Clear();
                hand.tiles.AddRange(tiles.Select(_ => new Tile(0, _, red: false)));

                foreach (var it in hand.tiles)
                {
                    var h = hand.Clone();
                    h.tiles.Remove(it);
                    var solution = h.Solve();
                    Assert.AreEqual(expected + 1, solution.向聴数);
                    Assert.IsTrue(h.向聴数IsLessThanOrEqual(expected + 1));
                }
            }
        }

        [Test]
        [TestCase(3900, TileType.M6, TileType.東, TileType.東, true,
            TileType.M5, TileType.M5, TileType.M6, TileType.M7, TileType.M7, TileType.P2, TileType.P3, TileType.P4, TileType.P6, TileType.P6, TileType.P5, TileType.P6, TileType.P7, TileType.M6)]
        public void 親ツモ(int expected, TileType ツモ牌, TileType ownWind, TileType roundWind,
            bool riichi,
            params TileType[] tiles)
        {
            var round = Game.Create(0, new RuleSetting()).ResetRound(tiles).Round;

            var hand = round.players[0].hand;
            hand.tiles.Clear();
            hand.tiles.AddRange(RandomUtil.GenerateShuffledArray(tiles.Select(_ => new Tile(0, _, red: false)).ToList()));
            var solution = hand.Solve();
            Assert.AreEqual(-1, solution.向聴数);

            var completed = solution.ChoiceCompletedHand(newTileInHand: ツモ牌, ownWind: ownWind, roundWind: roundWind,
                ronTarget: null,
                riichi: riichi,
                doubleRiichi: false,
                openRiichi: false,
                一発: false,
                嶺上: false,
                海底: false,
                河底: false,
                天和: false,
                地和: false,
                人和: false,
                doraTiles: new TileType[0],
                uraDoraTiles: new TileType[0],
                槍槓: false,
                handCap: round.game.rule.handCap);

            var player = round.Dealer;
            var r = CompletedHand.Execute((player, completed));
            var result = r.completedResults[player];
            Debug.Log(string.Join(", ", completed.Yakus.Keys.Concat(completed.役満.Keys)));
            Debug.Log(result.displayScore?.han + "翻 " + result.displayScore?.fu + "符 " + result.scoreType);
            Assert.AreEqual(expected, result.dealerTsumoPenalty);
            Assert.AreEqual(0, r.roundResult.scoreDiffs!.Values.Sum());
        }

        [Test]
        public void Number()
        {
            var count = 0;
            foreach (TileType it in System.Enum.GetValues(typeof(TileType)))
            {
                if (it.IsSuited())
                {
                    it.Number();
                    ++count;
                }
            }
            Assert.AreEqual(27, count);
        }

        [Test]
        [TestCase(8000, TileType.M1, TileType.東, TileType.東, true,
            new[] { TileType.M1, TileType.M1, TileType.M1, TileType.M3, TileType.M3, TileType.M3, TileType.P4, TileType.P4, TileType.P4, TileType.P5, TileType.P5, TileType.P5, TileType.M6, TileType.M6 },
            null)]
        [TestCase(1000, TileType.M1, TileType.東, TileType.東, false,
            new[] { TileType.M1, TileType.M2, TileType.M3, TileType.M6, TileType.M7, TileType.M8, TileType.P1, TileType.P2, TileType.P3, TileType.P4, TileType.P5, TileType.P6, TileType.S9, TileType.S9 },
            null)]
        [TestCase(1000, TileType.M2, TileType.東, TileType.東, false,
            new[] { TileType.M2, TileType.M3, TileType.M4, TileType.P2, TileType.P3, TileType.P4, TileType.P5, TileType.P6, TileType.P7, TileType.S8, TileType.S8 },
            new[] { TileType.M6, TileType.M7, TileType.M8 })]
        public void ロン上がり(int expected, TileType ロン牌, TileType ownWind, TileType roundWind,
            bool riichi,
            TileType[] tiles,
            TileType[]? meldTiles)
        {
            var round = Game.Create(0, new RuleSetting()).ResetRound(tiles).Round;

            var hand = round.players[0].hand;
            hand.tiles.Clear();
            hand.tiles.AddRange(RandomUtil.GenerateShuffledArray(tiles.Select(_ => new Tile(0, _, red: false)).ToList()));
            if (meldTiles != null)
            {
                for (int i = 0; i < meldTiles.Length / 3; ++i)
                {
                    var meld = new Meld(
                        (new Tile(0, meldTiles[i * 3], red: false), PlayerIndex.Index0),
                        (new Tile(0, meldTiles[i * 3 + 1], red: false), PlayerIndex.Index0),
                        (new Tile(0, meldTiles[i * 3 + 2], red: false), PlayerIndex.Index1));
                    hand.melds.Add(meld);
                }
            }
            var solution = hand.Solve();
            Assert.AreEqual(-1, solution.向聴数);

            var completed = solution.ChoiceCompletedHand(newTileInHand: ロン牌, ownWind: ownWind, roundWind: roundWind,
                ronTarget: round.players[0],
                riichi: riichi,
                doubleRiichi: false,
                openRiichi: false,
                一発: false,
                嶺上: false,
                海底: false,
                河底: false,
                天和: false,
                地和: false,
                人和: false,
                doraTiles: new TileType[0],
                uraDoraTiles: new TileType[0],
                槍槓: false,
                handCap: round.game.rule.handCap);

            var player = round.players[1];
            var r = CompletedHand.Execute((player, completed));
            var result = r.completedResults[player];
            Debug.Log(string.Join(", ", completed.Yakus.Keys.Concat(completed.役満.Keys)));
            Debug.Log(result.displayScore?.han + "翻 " + result.displayScore?.fu + "符 " + result.scoreType);
            Assert.AreEqual(expected, result.ronPenalty);
            Assert.AreEqual(0, r.roundResult.scoreDiffs!.Values.Sum());
        }
        [Test]
        public void HandWithMeld()
        {
            var tiles = new[]
            {
                TileType.M2, TileType.M3, TileType.M4,
                TileType.P5, TileType.P6,
                TileType.S8, TileType.S8,
            };
            var meld0 = new Meld(
                (new Tile(0, TileType.S3, false), PlayerIndex.Index0),
                (new Tile(0, TileType.S4, false), PlayerIndex.Index0),
                (new Tile(0, TileType.S5, false), PlayerIndex.Index1));
            var meld1 = new Meld(
                (new Tile(0, TileType.P3, false), PlayerIndex.Index0),
                (new Tile(0, TileType.P3, false), PlayerIndex.Index1),
                (new Tile(0, TileType.P3, false), PlayerIndex.Index0));

            var round = Game.Create(0, new RuleSetting()).Round;
            var hand = round.players[0].hand;
            hand.tiles.Clear();
            hand.tiles.AddRange(tiles.Select(_ => new Tile(0, _, false)));
            hand.melds.Add(meld0);
            hand.melds.Add(meld1);
            var solution = hand.Solve();
            Assert.AreEqual(0, solution.向聴数);
            Assert.IsTrue(hand.向聴数IsLessThanOrEqual(0));

            var winningTiles = hand.GetWinningTiles();
            Assert.AreEqual(new[] { TileType.P4, TileType.P7 }, winningTiles);

        }
    }
}