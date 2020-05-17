using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class GameResult
    {
        public readonly int[] sortedPlayerIndices;
        public readonly Dictionary<int, (int rank, double reward)> playerRanks;

        public GameResult(Game game)
        {
            var kaesi = game.rule.payment.返し;
            var umas = game.rule.payment.ウマ;

            // 同じ点数だった場合、席順で順位を決める
            var orderedPositionFromFirstOrder = new Dictionary<RelativePlayer, int>()
            {
                { RelativePlayer.自家, 3 },
                { RelativePlayer.下家, 2 },
                { RelativePlayer.対面, 1 },
                { RelativePlayer.上家, 0 },
            };

            sortedPlayerIndices = Enumerable.Range(0, game.scoreOwners.Length)
                .OrderByDescending(_ => game.scoreOwners[_].score)
                .ThenByDescending(_ => orderedPositionFromFirstOrder[RelativePlayerUtil.GetByPlayerIndex(game.firstDealer, _)])
                .ToArray();

            var rewards = new Dictionary<int, double>();
            var topPlayerIndex = sortedPlayerIndices[0];
            rewards[topPlayerIndex] = umas[0];

            for (int i = 0; i < sortedPlayerIndices.Length; ++i)
            {
                var playerIndex = sortedPlayerIndices[i];
                if (playerIndex != topPlayerIndex)
                {
                    var score = game.scoreOwners[playerIndex].score;
                    var p = System.Math.Round((score - kaesi) / 1000.0);
                    rewards[playerIndex] = p + umas[i];
                    rewards[topPlayerIndex] -= p;
                }
            }

            playerRanks = new Dictionary<int, (int rank, double reward)>();
            for (int i = 0; i < sortedPlayerIndices.Length; ++i)
            {
                var playerIndex = sortedPlayerIndices[i];
                playerRanks.Add(playerIndex, (rank: i + 1, reward: rewards[playerIndex]));
            }
        }
    }
}
