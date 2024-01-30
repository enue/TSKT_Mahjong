#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs
{
    public class AfterDraw : IController
    {
        public 局 局 => DrawPlayer.局;
        public bool Consumed { get; private set; }
        public PlayerIndex DrawPlayerIndex => DrawPlayer.index;
        public Player DrawPlayer { get; }

        public readonly Tile? newTileInHand;
        public readonly Hands.Solution? handSolution;
        public readonly bool canツモ上がり;

        public readonly bool 嶺上;
        public readonly bool openDoraAfterDiscard;

        bool 鳴きなし => 局.players.All(_ => _.手牌.副露.Count == 0);
        bool 一巡目 => DrawPlayer.捨て牌.Count == 0;
        bool Built副露 => newTileInHand == null;

        public AfterDraw(Player player, Tile? newTileInHand,
            bool 嶺上,
            bool openDoraAfterDiscard)
        {
            Debug.Assert(player.hand.tiles.Count % 3 == 2, "wrong hand tile count after draw");
            DrawPlayer = player;
            this.newTileInHand = newTileInHand;
            this.openDoraAfterDiscard = openDoraAfterDiscard;
            this.嶺上 = 嶺上;

            if (newTileInHand != null)
            {
                handSolution = DrawPlayer.手牌.Solve();
                if (handSolution.向聴数 == -1)
                {
                    canツモ上がり = !CompletedHand.役無し;
                }
            }
        }

        public CompletedHand CompletedHand
        {
            get
            {
                if (newTileInHand == null)
                {
                    throw new System.NullReferenceException();
                }
                return handSolution!.ChoiceCompletedHand(DrawPlayer, newTileInHand.type,
                    ronTarget: null,
                    嶺上: 嶺上,
                    海底: !嶺上 && (局.壁牌.tiles.Count == 0),
                    河底: false,
                    天和: 鳴きなし && 一巡目 && DrawPlayer.Is親,
                    地和: 鳴きなし && 一巡目 && !DrawPlayer.Is親,
                    人和: false,
                    槍槓: false);
            }
        }

        static public AfterDraw FromSerializable(in Serializables.AfterDraw source)
        {
            var round = source.round.Deserialize();
            var player = round.players[(int)source.drawPlayerIndex];
            var newTileInHand = source.newTileInHand >= 0 ? round.壁牌.allTiles[source.newTileInHand] : null;
            return new AfterDraw(player, newTileInHand, 嶺上: source.嶺上, openDoraAfterDiscard: source.openDoraAfterDiscard);
        }

        public Serializables.AfterDraw ToSerializable()
        {
            return new Serializables.AfterDraw(this);
        }
        public Serializables.Session SerializeSession()
        {
            return new Serializables.Session(this);
        }

        bool Canリーチ(out Commands.Discard[] commands)
        {
            var result = new List<Commands.Discard>();
            foreach (var it in DrawPlayer.手牌.tiles)
            {
                if (Canリーチ(it, out var command))
                {
                    result.Add(command);
                }
            }
            commands = result.ToArray();
            return commands.Length > 0;
        }

        bool Canリーチ(Tile tile, out Commands.Discard command)
        {
            if (DrawPlayer.リーチ)
            {
                command = default;
                return false;
            }
            if (局.game.rule.end.endWhenScoreUnderZero)
            {
                if (DrawPlayer.Score < 1000)
                {
                    command = default;
                    return false;
                }
            }
            if (!DrawPlayer.手牌.tiles.Contains(tile))
            {
                command = default;
                return false;
            }
            if (DrawPlayer.手牌.副露.Count > 0 && DrawPlayer.手牌.副露.Any(_ => !_.暗槓))
            {
                command = default;
                return false;
            }
            if (局.壁牌.tiles.Count == 0)
            {
                command = default;
                return false;
            }
            var cloneHand = DrawPlayer.手牌.Clone();
            cloneHand.tiles.Remove(tile);
            if (!cloneHand.向聴数IsLessThanOrEqual(0))
            {
                command = default;
                return false;
            }

            command = new Commands.Discard(this, tile, リーチ: true, オープンリーチ: false);
            return true;
        }

        bool Canオープンリーチ(out Commands.Discard[] commands)
        {
            if (局.game.rule.openRiichi == Rules.OpenRiichi.なし)
            {
                commands = System.Array.Empty<Commands.Discard>();
                return false;
            }

            var result = new List<Commands.Discard>();
            foreach (var it in DrawPlayer.手牌.tiles)
            {
                if (Canオープンリーチ(it, out var command))
                {
                    result.Add(command);
                }
            }
            commands = result.ToArray();
            return commands.Length > 0;
        }

        bool Canオープンリーチ(Tile tile, out Commands.Discard command)
        {
            if (局.game.rule.openRiichi == Rules.OpenRiichi.なし)
            {
                command = default;
                return false;
            }

            if (Canリーチ(tile, out var riichi))
            {
                command = new Commands.Discard(riichi.Controller, riichi.tile, true, true);
                return true;
            }
            command = default;
            return false;
        }

        bool CanDiscard(out Commands.Discard[] commands)
        {
            var result = new List<Commands.Discard>();
            foreach (var it in DrawPlayer.手牌.tiles)
            {
                if (CanDiscard(it, out var command))
                {
                    result.Add(command);
                }
            }
            commands = result.ToArray();
            return commands.Length > 0;
        }

        bool CanDiscard(Tile tile, out Commands.Discard command)
        {
            if (tile == newTileInHand)
            {
                command = new Commands.Discard(this, tile, リーチ: false, オープンリーチ: false);
                return true;
            }

            if (DrawPlayer.リーチ)
            {
                command = default;
                return false;
            }

            if (DrawPlayer.局.game.rule.喰い替え == Rules.喰い替え.なし)
            {
                if (Built副露)
                {
                    var meld = DrawPlayer.手牌.副露.Last();
                    if (meld.Is喰い替え(tile))
                    {
                        command = default;
                        return false;
                    }
                }
            }
            if (!DrawPlayer.手牌.tiles.Contains(tile))
            {
                command = default;
                return false;
            }

            command = new Commands.Discard(this, tile, リーチ: false, オープンリーチ: false);
            return true;
        }

        bool Canツモ上がり(out Commands.ツモ上がり command)
        {
            if (!canツモ上がり)
            {
                command = default;
                return false;
            }

            command = new Commands.ツモ上がり(this);
            return true;
        }

        bool Can暗槓(out Commands.暗槓[] commands)
        {
            var result = new List<Commands.暗槓>();
            foreach (var tile in DrawPlayer.手牌.tiles.Select(_ => _.type).Distinct())
            {
                if (Can暗槓(tile, out var command))
                {
                    result.Add(command);
                }
            }

            commands = result.ToArray();
            return commands.Length > 0;
        }

        bool Can暗槓(TileType tile, out Commands.暗槓 command)
        {
            // 海底はカンできない
            if (局.壁牌.tiles.Count == 0)
            {
                command = default;
                return false;
            }
            // 鳴いた直後にカンはできない
            if (Built副露)
            {
                command = default;
                return false;
            }
            // 暗槓は立直後でもできるが、ツモ牌でのみ
            if (DrawPlayer.リーチ)
            {
                if (newTileInHand?.type != tile)
                {
                    command = default;
                    return false;
                }
            }
            if (!DrawPlayer.CanClosedQuad(tile))
            {
                command = default;
                return false;
            }

            command = new Commands.暗槓(this, tile);
            return true;
        }

        bool Can加槓(out Commands.加槓[] commands)
        {
            var result = new List<Commands.加槓>();
            foreach (var it in DrawPlayer.手牌.tiles.Select(_ => _.type).Distinct())
            {
                if (Can加槓(it, out var command))
                {
                    result.Add(command);
                }
            }
            commands = result.ToArray();
            return commands.Length > 0;
        }

        bool Can加槓(TileType tile, out Commands.加槓 command)
        {
            // 海底はカンできない
            if (局.壁牌.tiles.Count == 0)
            {
                command = default;
                return false;
            }
            // 鳴いた直後にカンはできない
            if (Built副露)
            {
                command = default;
                return false;
            }
            if (!DrawPlayer.CanAddedOpenQuad(tile))
            {
                command = default;
                return false;
            }
            var t = DrawPlayer.手牌.tiles.First(_ => _.type == tile);
            command = new Commands.加槓(this, t);
            return true;
        }
        bool Can九種九牌(out Commands.九種九牌 command)
        {
            if (!鳴きなし)
            {
                command = default;
                return false;
            }
            if (!一巡目)
            {
                command = default;
                return false;
            }

            if (DrawPlayer.手牌.tiles
                .Select(_ => _.type)
                .Where(_ => _.么九牌())
                .Distinct()
                .Count() < 9)
            {
                command = default;
                return false;
            }

            command = new Commands.九種九牌(this);
            return true;
        }

        public AfterDraw ResetRound(params TileType[]?[]? initialPlayerTilesByCheat)
        {
            return 局.game.StartRound(initialPlayerTilesByCheat);
        }

        public AfterDraw? DoDefaultAction(out RoundResult? roundResult)
        {
            throw new System.NotImplementedException();
        }

        public ClaimingCommandSet GetExecutableClaimingCommandsBy(Player player)
        {
            return default;
        }
        public DiscardingCommandSet GetExecutableDiscardingCommandsBy(Player player)
        {
            if (player == DrawPlayer)
            {
                Can暗槓(out var declareCloseQuads);
                Can加槓(out var declareAddedOpenQuads);
                CanDiscard(out var discards);
                Canリーチ(out var riichies);
                Canオープンリーチ(out var openRiichies);
                Commands.ツモ上がり? tsumo;
                if (Canツモ上がり(out var t))
                {
                    tsumo = t;
                }
                else
                {
                    tsumo = null;
                }
                Commands.九種九牌? nineTiles;
                if (Can九種九牌(out var n))
                {
                    nineTiles = n;
                }
                else
                {
                    nineTiles = null;
                }
                return new DiscardingCommandSet(加槓: declareAddedOpenQuads, 暗槓: declareCloseQuads,
                        discards: discards, riichies: riichies, openRiichies: openRiichies,
                        ツモ上がり: tsumo, nineTiles: nineTiles);
            }
            else
            {
                return default;
            }
        }


        public ICommand[] ExecutableCommands
        {
            get
            {
                var result = new List<ICommand>();

                if (Can暗槓(out var closedQuads))
                {
                    result.AddRange(closedQuads.Cast<ICommand>());
                }
                if (Can加槓(out var addedOpenQuads))
                {
                    result.AddRange(addedOpenQuads.Cast<ICommand>());
                }
                if (CanDiscard(out var discards))
                {
                    result.AddRange(discards.Cast<ICommand>());
                }
                if (Canリーチ(out var riichies))
                {
                    result.AddRange(riichies.Cast<ICommand>());
                }
                if (Canオープンリーチ(out var openRiichies))
                {
                    result.AddRange(openRiichies.Cast<ICommand>());
                }
                if (Canツモ上がり(out var tsumo))
                {
                    result.Add(tsumo);
                }
                if (Can九種九牌(out var nineTiles))
                {
                    result.Add(nineTiles);
                }

                return result.ToArray();
            }
        }
        public CommandResult ExecuteCommands(out List<ICommand> executedCommands, params ICommand[] commands)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;
            var selector = new CommandSelector(this);
            return selector.Execute(out executedCommands, commands);
        }
    }
}
