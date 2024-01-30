#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs.Hands
{
    public class Solution
    {
        public readonly int 向聴数;
        public readonly Structure[] structures;

        public Solution(手牌 hand)
        {
            (向聴数, structures) = Structure.Build(hand);
        }

        public 和了 Choice和了(Player player,
            TileType newTileInHand,
            Player? ronTarget,
            bool 嶺上,
            bool 海底,
            bool 河底,
            bool 天和,
            bool 地和,
            bool 人和,
            bool 槍槓)
        {
            return Choice和了(
                newTileInHand,
                ownWind: player.自風,
                roundWind: player.局.場風,
                ronTarget: ronTarget,
                riichi: player.リーチ,
                doubleRiichi: player.ダブルリーチ,
                openRiichi: player.オープンリーチ,
                一発: player.一発,
                嶺上: 嶺上,
                海底: 海底,
                河底: 河底,
                天和: 天和,
                地和: 地和,
                人和: 人和,
                doraTiles: player.局.王牌.DoraTiles,
                uraDoraTiles: player.局.王牌.UraDoraTiles,
                槍槓: 槍槓,
                handCap: player.局.game.rule.役満複合);
        }

        public 和了 Choice和了(TileType newTileInHand, TileType ownWind, TileType roundWind,
            Player? ronTarget,
            bool riichi,
            bool doubleRiichi,
            bool openRiichi,
            bool 一発,
            bool 嶺上,
            bool 海底,
            bool 河底,
            bool 天和,
            bool 地和,
            bool 人和,
            TileType[] doraTiles,
            TileType[] uraDoraTiles,
            bool 槍槓,
            Rules.役満複合上限 handCap)
        {
            var result = (score: int.MinValue, completed: default(和了));

            foreach (var it in structures)
            {
                var item = new 和了(it, newTileInHand, ownWind: ownWind, roundWind: roundWind,
                    ronTarget: ronTarget,
                    riichi: riichi,
                    doubleRiichi: doubleRiichi,
                    openRiichi: openRiichi,
                    一発: 一発,
                    嶺上: 嶺上,
                    海底: 海底,
                    河底: 河底,
                    天和: 天和,
                    地和: 地和,
                    人和: 人和,
                    槍槓: 槍槓,
                    doraTiles: doraTiles,
                    uraDoraTiles: uraDoraTiles);
                if (result.score < item.基本点(handCap).score)
                {
                    result = (item.基本点(handCap).score, item);
                }
            }

            return result.completed;
        }
    }
}
