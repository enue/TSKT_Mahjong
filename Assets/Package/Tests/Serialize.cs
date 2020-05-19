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
        public void MonkeyTest()
        {
            IController controller = Game.Create(0, new RuleSetting());
            for (int i = 0; i < 1000; ++i)
            {
                var jsonString = controller.SerializeSession().ToJson();
                controller = TSKT.Mahjongs.Serializables.Session.FromJson(jsonString);
                var jsonString2 = controller.SerializeSession().ToJson();
                Assert.AreEqual(jsonString, jsonString2);

                var selector = new CommandSelector(controller);
                var commands = controller.ExecutableCommands;
                if (commands.Length > 0)
                {
                    var command = commands[Random.Range(0, commands.Length)];
                    selector.commands.Add(command);
                }
                var result = selector.Execute(out var _);
                controller = result.nextController;

                if (controller == null)
                {
                    break;
                }
            }
        }
    }
}
