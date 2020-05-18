using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class BeforeClosedQuad : IBeforeQuad
    {
        public Round Round => DeclarePlayer.round;
        public int DeclarePlayerIndex => DeclarePlayer.index;
        public Player DeclarePlayer { get; }

        public bool Consumed { get; private set; }

        public readonly TileType tile;
        public Dictionary<Player, CompletedHand> PlayerRons { get; } = new Dictionary<Player, CompletedHand>();

        public BeforeClosedQuad(Player declarePlayer, TileType tile)
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
                hand.tiles.Add(new Tile(0, tile, false));
                var solution = hand.Solve();
                if (solution.向聴数 > -1)
                {
                    continue;
                }
                var completed = solution.ChoiceCompletedHand(ronPlayer, tile,
                    ronTarget: declarePlayer,
                    嶺上: false,
                    海底: false,
                    河底: false,
                    天和: false,
                    地和: false,
                    人和: false,
                    槍槓: false);
                if (completed.役満.ContainsKey(役.国士無双))
                {
                    PlayerRons.Add(ronPlayer, completed);
                }
            }
        }

        public bool CanRon(Player player)
        {
            return PlayerRons.ContainsKey(player);
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

            return CompletedHand.Execute(players.ToDictionary(_ => _, _ => PlayerRons[_]),
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

            return Round.ExecuteClosedQuad(DeclarePlayer, tile);
        }

        public IController DoDefaultAction(out RoundResult roundResult)
        {
            roundResult = null;
            return BuildQuad();
        }

        public ICommand[] ExecutableCommands
        {
            get
            {
                var result = new List<ICommand>();
                foreach (var player in Round.players)
                {
                    if (CanRon(player))
                    {
                        result.Add(new Commands.槍槓(player, this));
                    }
                }
                return result.ToArray();
            }
        }
    }
}
