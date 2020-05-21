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
        public Dictionary<Player, CompletedHand> PlayerRons { get; } = new Dictionary<Player, CompletedHand>();

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
                if (ronPlayer.Furiten)
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
                    PlayerRons.Add(ronPlayer, completed);
                }
            }
        }

        public static BeforeAddedOpenQuad FromSerializable(Serializables.BeforeAddedOpenQuad source)
        {
            var round = source.round.Deserialzie();
            var player = round.players[source.declarePlayerIndex];
            var tile = round.wallTile.allTiles[source.tile];
            return new BeforeAddedOpenQuad(player, tile);
        }
        public Serializables.BeforeAddedOpenQuad ToSerializable()
        {
            return new Serializables.BeforeAddedOpenQuad(this);
        }
        public Serializables.Session SerializeSession()
        {
            return new Serializables.Session(this);
        }

        public bool CanRon(out Commands.Ron[] commands)
        {
            commands = PlayerRons.Select(_ => new Commands.Ron(_.Key, this)).ToArray();
            return commands.Length > 0;
        }

        public bool CanRon(Player player, out Commands.Ron command)
        {
            if (!PlayerRons.ContainsKey(player))
            {
                command = null;
                return false;
            }
            command = new Commands.Ron(player, this);
            return true;
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

            foreach (var it in Round.players)
            {
                it.一発 = false;
            }

            return Round.ExecuteAddedOpenQuad(DeclarePlayer, tile);
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

                if (CanRon(out var rons))
                {
                    result.AddRange(rons);
                }
                return result.ToArray();
            }
        }
        public CommandResult ExecuteCommands(out List<ICommand> executedCommands, params ICommand[] commands)
        {
            var selector = new CommandSelector(this);
            selector.commands.AddRange(commands);
            return selector.Execute(out executedCommands);
        }
    }
}
