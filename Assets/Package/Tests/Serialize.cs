using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using TSKT.Mahjongs;
using TSKT.Mahjongs.Rules;
using System.Linq;

namespace TSKT.Tests.Mahjongs
{
    public class Serialzie
    {
        [Test]
        public void ToJson()
        {
            var controller0_0 = Game.Create(0, new RuleSetting());
            var session0_0 = controller0_0.SerializeSession();
            var json0_0 = session0_0.ToJson(prettyPrint: true);
            Debug.Log(json0_0);

            var controller0_1 = TSKT.Mahjongs.Serializables.Session.FromJson(json0_0);
            Assert.AreEqual(controller0_1.GetType(), controller0_0.GetType());

            var json0_1 = controller0_1.SerializeSession().ToJson(prettyPrint: true);
            Assert.AreEqual(json0_0, json0_1);


            var controller1_0 = controller0_0.Discard(controller0_0.DrawPlayer.hand.tiles[0], false);
            var session1_0 = controller1_0.SerializeSession();
            var json1_0 = session1_0.ToJson();
            var controller1_1 = TSKT.Mahjongs.Serializables.Session.FromJson(json1_0);
            Assert.AreEqual(controller1_0.GetType(), controller1_1.GetType());
            var session1_2 = controller1_1.SerializeSession();
            var json1_1 = session1_2.ToJson();
            Assert.AreEqual(json1_0, json1_1);
        }
        [Test]
        [TestCase(true, OpenRiichi.あり, HandCap.トリプル役満, RedTile.赤ドラ4, 喰い替え.なし, 四家立直.流局, TripleRon.有効, 明槓槓ドラ.打牌後)]
        [TestCase(false, OpenRiichi.あり, HandCap.トリプル役満, RedTile.赤ドラ4, 喰い替え.なし, 四家立直.流局, TripleRon.有効, 明槓槓ドラ.打牌後)]
        [TestCase(true, OpenRiichi.なし, HandCap.役満, RedTile.赤無し, 喰い替え.あり, 四家立直.続行, TripleRon.流局, 明槓槓ドラ.即ノリ)]
        [TestCase(true, OpenRiichi.あり, HandCap.ダブル役満, RedTile.赤ドラ3, 喰い替え.なし, 四家立直.流局, TripleRon.有効, 明槓槓ドラ.打牌後)]
        public void MonkeyTest(bool serialize,
            OpenRiichi openRiichi,
            HandCap handCap,
            RedTile redTile,
            喰い替え kuikae,
            四家立直 allRiichi,
            TripleRon tripleRon,
            明槓槓ドラ 明槓槓ドラ)
        {
            IController controller = Game.Create(0, new RuleSetting()
            {
                openRiichi = openRiichi,
                handCap = handCap,
                redTile = redTile,
                喰い替え = kuikae,
                四家立直 = allRiichi,
                tripleRon = tripleRon,
                明槓槓ドラ = 明槓槓ドラ
            });
            for (int i = 0; i < 1000; ++i)
            {
                if (serialize)
                {
                    var jsonString = controller.SerializeSession().ToJson();
                    controller = TSKT.Mahjongs.Serializables.Session.FromJson(jsonString);
                    var jsonString2 = controller.SerializeSession().ToJson();
                    Assert.AreEqual(jsonString, jsonString2);
                }

                var commands = controller.ExecutableCommands;
                CommandResult result;
                if (commands.Length > 0)
                {
                    var command = commands[Random.Range(0, commands.Length)];
                    result = controller.ExecuteCommands(out _, command);
                }
                else
                {
                    result = controller.ExecuteCommands(out _);
                }
                controller = result.nextController;

                if (controller == null)
                {
                    break;
                }
            }
        }

        [Test]
        public void AutoBetterMonkeyTest()
        {
            var tasks = new List<System.Threading.Tasks.Task>();
            foreach (OpenRiichi openRiichi in System.Enum.GetValues(typeof(OpenRiichi)))
            {
                foreach (HandCap handCap in System.Enum.GetValues(typeof(HandCap)))
                {
                    foreach (RedTile redTile in System.Enum.GetValues(typeof(RedTile)))
                    {
                        foreach (喰い替え 喰い替え in System.Enum.GetValues(typeof(喰い替え)))
                        {
                            foreach (四家立直 四家立直 in System.Enum.GetValues(typeof(四家立直)))
                            {
                                foreach (TripleRon tripleRon in System.Enum.GetValues(typeof(TripleRon)))
                                {
                                    foreach (明槓槓ドラ 明槓槓ドラ in System.Enum.GetValues(typeof(明槓槓ドラ)))
                                    {
                                        foreach (アガリ止め アガリ止め in System.Enum.GetValues(typeof(アガリ止め)))
                                        {
                                            var task = System.Threading.Tasks.Task.Run(() =>
                                            {
                                                BetterMonkeyTest(false,
                                                    openRiichi, handCap, redTile, 喰い替え, 四家立直, tripleRon, 明槓槓ドラ, アガリ止め);
                                            });
                                            tasks.Add(task);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (var it in tasks)
            {
                it.Wait();
            }
        }

        [Test]
        [TestCase(true, OpenRiichi.あり, HandCap.トリプル役満, RedTile.赤ドラ4, 喰い替え.なし, 四家立直.流局, TripleRon.有効, 明槓槓ドラ.打牌後, アガリ止め.なし)]
        [TestCase(true, OpenRiichi.あり, HandCap.トリプル役満, RedTile.赤ドラ4, 喰い替え.なし, 四家立直.流局, TripleRon.有効, 明槓槓ドラ.打牌後, アガリ止め.なし)]
        [TestCase(true, OpenRiichi.なし, HandCap.役満, RedTile.赤無し, 喰い替え.あり, 四家立直.続行, TripleRon.流局, 明槓槓ドラ.即ノリ, アガリ止め.あり)]
        [TestCase(true, OpenRiichi.あり, HandCap.ダブル役満, RedTile.赤ドラ3, 喰い替え.なし, 四家立直.流局, TripleRon.有効, 明槓槓ドラ.打牌後, アガリ止め.あり)]
        public void BetterMonkeyTest(
            bool verbose,
            OpenRiichi openRiichi,
            HandCap handCap,
            RedTile redTile,
            喰い替え kuikae,
            四家立直 allRiichi,
            TripleRon tripleRon,
            明槓槓ドラ 明槓槓ドラ,
            アガリ止め アガリ止め)
        {
            var executedCount = new Dictionary<System.Type, int>();

            IController controller = Game.Create(0, new RuleSetting()
            {
                end = new EndRule()
                {
                    アガリ止め = アガリ止め,
                },
                openRiichi = openRiichi,
                handCap = handCap,
                redTile = redTile,
                喰い替え = kuikae,
                四家立直 = allRiichi,
                tripleRon = tripleRon,
                明槓槓ドラ = 明槓槓ドラ,
            });
            for (int i = 0; i < 1000; ++i)
            {
                var commands = controller.ExecutableCommands;
                CommandResult result;
                if (commands.Length > 0)
                {
                    RandomUtil.Shuffle(ref commands);
                    var discards = commands.OfType<TSKT.Mahjongs.Commands.Discard>()
                        .OrderBy(_ => _.HandAfterDiscard.Solve().向聴数);
                    if (verbose)
                    {
                        foreach (var discard in discards)
                        {
                            var winningTilesHiddenCount = discard.WinningTilesHiddenCount;
                            var tilesToShow = discard.TilesToShowWhenOpenRiichi;
                            if (tilesToShow.Length > 0)
                            {
                                var handTiles = discard.HandAfterDiscard.tiles.Select(_ => _.type).OrderBy(_ => _);
                                Debug.Log(
                                    string.Join(", ", winningTilesHiddenCount)
                                    + ", hand : " + string.Join(", ", handTiles)
                                    + ", related : " + string.Join(", ", tilesToShow));
                            }
                        }
                    }

                    var c = discards.Cast<ICommand>().Concat(commands).ToArray();

                    result = controller.ExecuteCommands(out var executeds, c);

                    foreach (var it in executeds)
                    {
                        var type = it.GetType();
                        executedCount.TryGetValue(type, out var count);
                        executedCount[type] = count + 1;
                    }
                }
                else
                {
                    result = controller.ExecuteCommands(out _);
                }
                controller = result.nextController;

                if (controller == null)
                {
                    break;
                }
            }

            if (verbose)
            {
                Debug.Log(string.Join("\n", executedCount.Select(_ => _.Key.ToString() + ", " + _.Value.ToString())));
            }
        }
    }
}
