using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using TSKT.Mahjongs;
using TSKT.Mahjongs.Rules;
using System.Linq;
#nullable enable

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

            var discards = controller0_0.GetExecutableDiscardingCommandsBy(controller0_0.DrawPlayer).Discards;
            var controller1_0 = discards[0].Execute().nextController;
            var session1_0 = controller1_0.SerializeSession();
            var json1_0 = session1_0.ToJson();
            var controller1_1 = TSKT.Mahjongs.Serializables.Session.FromJson(json1_0);
            Assert.AreEqual(controller1_0.GetType(), controller1_1.GetType());
            var session1_2 = controller1_1.SerializeSession();
            var json1_1 = session1_2.ToJson();
            Assert.AreEqual(json1_0, json1_1);
        }
        [Test]
        [TestCase(true, オープンリーチ.あり, 役満複合上限.トリプル役満, 赤牌.赤ドラ4, 喰い替え.なし, 四家立直.流局, トリロン.有効, 明槓槓ドラ.打牌後)]
        [TestCase(false, オープンリーチ.あり, 役満複合上限.トリプル役満, 赤牌.赤ドラ4, 喰い替え.なし, 四家立直.流局, トリロン.有効, 明槓槓ドラ.打牌後)]
        [TestCase(true, オープンリーチ.なし, 役満複合上限.役満, 赤牌.赤無し, 喰い替え.あり, 四家立直.続行, トリロン.流局, 明槓槓ドラ.即ノリ)]
        [TestCase(true, オープンリーチ.あり, 役満複合上限.ダブル役満, 赤牌.赤ドラ3, 喰い替え.なし, 四家立直.流局, トリロン.有効, 明槓槓ドラ.打牌後)]
        public void MonkeyTest(bool serialize,
            オープンリーチ openRiichi,
            役満複合上限 handCap,
            赤牌 redTile,
            喰い替え kuikae,
            四家立直 allRiichi,
            トリロン tripleRon,
            明槓槓ドラ 明槓槓ドラ)
        {
            IController controller = Game.Create(0, new RuleSetting()
            {
                オープンリーチ = openRiichi,
                役満複合 = handCap,
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
                if (result.nextController == null)
                {
                    break;
                }
                controller = result.nextController;
            }
        }
        [Test]
        public void Rule()
        {
            var rule = new RuleSetting();
            rule.payment.返し = 1000;
            rule.payment.ウマ[0] = 99;
            rule.end.lengthType = LengthType.一荘戦;
            var controller0 = Game.Create(0, rule);

            var json = controller0.SerializeSession().ToJson();
            var controller1 = TSKT.Mahjongs.Serializables.Session.FromJson(json);
            Assert.AreEqual(JsonUtility.ToJson(rule.end), JsonUtility.ToJson(controller1.局.game.rule.end));
            Assert.AreEqual(JsonUtility.ToJson(rule.payment), JsonUtility.ToJson(controller1.局.game.rule.payment));
        }
    }
}
