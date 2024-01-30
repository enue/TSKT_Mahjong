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
    public class BetterMonkeyTest
    {
        [Test]
        public void Rules()
        {
            foreach (オープンリーチ openRiichi in System.Enum.GetValues(typeof(オープンリーチ)))
            {
                Execute(false, openRiichi, default, default, default, default, default, default, default);
            }
            foreach (役満複合上限 handCap in System.Enum.GetValues(typeof(役満複合上限)))
            {
                Execute(false, default, handCap, default, default, default, default, default, default);
            }
            foreach (赤牌 redTile in System.Enum.GetValues(typeof(赤牌)))
            {
                Execute(false, default, default, redTile, default, default, default, default, default);
            }
            foreach (喰い替え 喰い替え in System.Enum.GetValues(typeof(喰い替え)))
            {
                Execute(false, default, default, default, 喰い替え, default, default, default, default);
            }
            foreach (四家立直 四家立直 in System.Enum.GetValues(typeof(四家立直)))
            {
                Execute(false, default, default, default, default, 四家立直, default, default, default);
            }
            foreach (トリロン tripleRon in System.Enum.GetValues(typeof(トリロン)))
            {
                Execute(false, default, default, default, default, default, tripleRon, default, default);
            }
            foreach (明槓槓ドラ 明槓槓ドラ in System.Enum.GetValues(typeof(明槓槓ドラ)))
            {
                Execute(false, default, default, default, default, default, default, 明槓槓ドラ, default);
            }
            foreach (アガリ止め アガリ止め in System.Enum.GetValues(typeof(アガリ止め)))
            {
                Execute(false, default, default, default, default, default, default, default, アガリ止め);
            }
        }

        [Test]
        [TestCase(true, オープンリーチ.あり, 役満複合上限.トリプル役満, 赤牌.赤ドラ4, 喰い替え.なし, 四家立直.流局, トリロン.有効, 明槓槓ドラ.打牌後, アガリ止め.なし)]
        [TestCase(true, オープンリーチ.あり, 役満複合上限.トリプル役満, 赤牌.赤ドラ4, 喰い替え.なし, 四家立直.流局, トリロン.有効, 明槓槓ドラ.打牌後, アガリ止め.なし)]
        [TestCase(true, オープンリーチ.なし, 役満複合上限.役満, 赤牌.赤無し, 喰い替え.あり, 四家立直.続行, トリロン.流局, 明槓槓ドラ.即ノリ, アガリ止め.あり)]
        [TestCase(true, オープンリーチ.あり, 役満複合上限.ダブル役満, 赤牌.赤ドラ3, 喰い替え.なし, 四家立直.流局, トリロン.有効, 明槓槓ドラ.打牌後, アガリ止め.あり)]
        public void Execute(
            bool verbose,
            オープンリーチ openRiichi,
            役満複合上限 handCap,
            赤牌 redTile,
            喰い替え kuikae,
            四家立直 allRiichi,
            トリロン tripleRon,
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
                オープンリーチ = openRiichi,
                役満複合 = handCap,
                redTile = redTile,
                喰い替え = kuikae,
                四家立直 = allRiichi,
                tripleRon = tripleRon,
                明槓槓ドラ = 明槓槓ドラ,
            });
            for (int i = 0; i < 1000; ++i)
            {
                foreach (var player in controller.局.players)
                {
                    var set = controller.GetExecutableClaimingCommandsBy(player);
                    var discarding = controller.GetExecutableDiscardingCommandsBy(player);
                    var pons = set.DistinctPons;
                    var chies = set.DistinctChies;
                }
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
                if (result.nextController == null)
                {
                    break;
                }
                controller = result.nextController;
            }

            if (verbose)
            {
                Debug.Log(string.Join("\n", executedCount.Select(_ => _.Key.ToString() + ", " + _.Value.ToString())));
            }
        }
    }
}
