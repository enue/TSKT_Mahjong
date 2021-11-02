#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs
{
    /// <summary>
    /// 暗槓宣言時。国士無双ロンができる
    /// </summary>
    public class BeforeClosedQuad : IBeforeQuad
    {
        public Round Round => DeclarePlayer.round;
        public PlayerIndex DeclarePlayerIndex => DeclarePlayer.index;
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
                if (ronPlayer.Furiten)
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

        public static BeforeClosedQuad FromSerializable(in Serializables.BeforeClosedQuad source)
        {
            var round = source.round.Deserialzie();
            var player = round.players[(int)source.declarePlayerIndex];
            return new BeforeClosedQuad(player, source.tile);
        }

        public Serializables.BeforeClosedQuad ToSerializable()
        {
            return new Serializables.BeforeClosedQuad(this);
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
                command = default;
                return false;
            }
            command = new Commands.Ron(player, this);
            return true;
        }
        public AfterDraw? Ron(
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

            // 国士無双の槍槓見逃しでフリテンになるが、どのみち上がれないので無視する
            foreach (var it in Round.players)
            {
                it.一発 = false;
            }

            return Round.ExecuteClosedQuad(DeclarePlayer, tile);
        }

        public AfterDraw? DoDefaultAction(out RoundResult? roundResult)
        {
            roundResult = null;
            return BuildQuad();
        }

        public void GetExecutableCommands(out Commands.Ron[] rons)
        {
            CanRon(out rons);
        }

        public ICommand[] ExecutableCommands
        {
            get
            {
                var result = new List<ICommand>();
                if (CanRon(out var rons))
                {
                    result.AddRange(rons.Cast<ICommand>());
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
