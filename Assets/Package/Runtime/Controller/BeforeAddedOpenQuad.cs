using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class BeforeAddedOpenQuad : IBeforeQuad
    {
        public Round Round => DeclarePlayer.round;
        public int DeclarePlayerIndex => DeclarePlayer.index;
        public Player DeclarePlayer { get; }

        public bool Consumed { get; private set; }

        public readonly Tile tile;
        public readonly Dictionary<Player, CompletedHand> rons = new Dictionary<Player, CompletedHand>();

        public BeforeAddedOpenQuad(Player declarePlayer, Tile tile)
        {
            DeclarePlayer = declarePlayer;
            this.tile = tile;

            foreach (var ronPlayer in Round.players)
            {
                if (ronPlayer == DeclarePlayer)
                {
                    continue;
                }
                var hand = ronPlayer.hand.Clone();
                hand.tiles.Add(tile);
                var solution = hand.Solve();
                if (solution.向聴数 > -1)
                {
                    continue;
                }
                var completed = solution.ChoiceCompletedHand(ronPlayer, tile.type,
                    ronTarget: declarePlayer,
                    嶺上: false,
                    海底: false,
                    河底: false,
                    天和: false,
                    地和: false,
                    人和: false,
                    槍槓: true);
                if (!completed.役無し)
                {
                    rons.Add(ronPlayer, completed);
                }
            }
        }

        public bool CanRon(Player player)
        {
            return rons.ContainsKey(player);
        }
        public AfterDraw Ron(
            out RoundResult roundResult,
            out Dictionary<Player, CompletedResult> result,
            params Player[] players)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            return CompletedHand.Execute(players.ToDictionary(_ => _, _ => rons[_]),
                out roundResult,
                out result);
        }

        public AfterDraw BuildQuad()
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            return Round.ExecuteAddedOpenQuad(DeclarePlayer, tile);
        }

        public IController DoDefaultAction(out RoundResult roundResult)
        {
            roundResult = null;
            return BuildQuad();
        }
    }
}
