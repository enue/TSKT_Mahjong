using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#nullable enable

namespace TSKT.Mahjongs
{
    public class AfterDraw : IController
    {
        public Round Round => DrawPlayer.round;
        public PlayerIndex DrawPlayerIndex => DrawPlayer.index;
        public Player DrawPlayer { get; }

        public bool Consumed { get; private set; }

        public readonly Tile? newTileInHand;
        public readonly Hands.Solution? handSolution;
        public readonly bool canTsumo;

        public readonly bool 嶺上;
        public readonly bool openDoraAfterDiscard;

        bool 鳴きなし => Round.players.All(_ => _.hand.melds.Count == 0);
        bool 一巡目 => DrawPlayer.discardedTiles.Count == 0;
        bool BuiltMeld => newTileInHand == null;

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
                handSolution = DrawPlayer.hand.Solve();
                if (handSolution.向聴数 == -1)
                {
                    canTsumo = !CompletedHand.役無し;
                }
            }
        }

        CompletedHand CompletedHand
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
                    海底: !嶺上 && (Round.wallTile.tiles.Count == 0),
                    河底: false,
                    天和: 鳴きなし && 一巡目 && DrawPlayer.IsDealer,
                    地和: 鳴きなし && 一巡目 && !DrawPlayer.IsDealer,
                    人和: false,
                    槍槓: false);
            }
        }

        static public AfterDraw FromSerializable(in Serializables.AfterDraw source)
        {
            var round = source.round.Deserialzie();
            var player = round.players[(int)source.drawPlayerIndex];
            var newTileInHand = source.newTileInHand >= 0 ? round.wallTile.allTiles[source.newTileInHand] : null;
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

        public bool CanRiichi(out Commands.Discard[] commands)
        {
            var result = new List<Commands.Discard>();
            foreach (var it in DrawPlayer.hand.tiles)
            {
                if (CanRiichi(it, out var command))
                {
                    result.Add(command);
                }
            }
            commands = result.ToArray();
            return commands.Length > 0;
        }

        public bool CanRiichi(Tile tile, out Commands.Discard command)
        {
            if (DrawPlayer.Riichi)
            {
                command = default;
                return false;
            }
            if (Round.game.rule.end.endWhenScoreUnderZero)
            {
                if (DrawPlayer.scoreOwner.score < 1000)
                {
                    command = default;
                    return false;
                }
            }
            if (!DrawPlayer.hand.tiles.Contains(tile))
            {
                command = default;
                return false;
            }
            if (DrawPlayer.hand.melds.Count > 0 && DrawPlayer.hand.melds.Any(_ => !_.暗槓))
            {
                command = default;
                return false;
            }
            if (Round.wallTile.tiles.Count == 0)
            {
                command = default;
                return false;
            }
            var cloneHand = DrawPlayer.hand.Clone();
            cloneHand.tiles.Remove(tile);
            if (!cloneHand.向聴数IsLessThanOrEqual(0))
            {
                command = default;
                return false;
            }

            command = new Commands.Discard(this, tile, riichi: true, openRiichi: false);
            return true;
        }

        public bool CanOpenRiichi(out Commands.Discard[]? commands)
        {
            if (Round.game.rule.openRiichi == Rules.OpenRiichi.なし)
            {
                commands = null;
                return false;
            }

            var result = new List<Commands.Discard>();
            foreach (var it in DrawPlayer.hand.tiles)
            {
                if (CanOpenRiichi(it, out var command))
                {
                    result.Add(command);
                }
            }
            commands = result.ToArray();
            return commands.Length > 0;
        }

        public bool CanOpenRiichi(Tile tile, out Commands.Discard command)
        {
            if (Round.game.rule.openRiichi == Rules.OpenRiichi.なし)
            {
                command = default;
                return false;
            }

            if (CanRiichi(tile, out var riichi))
            {
                command = new Commands.Discard(riichi.Controller, riichi.tile, true, true);
                return true;
            }
            command = default;
            return false;
        }

        public bool CanDiscard(out Commands.Discard[] commands)
        {
            var result = new List<Commands.Discard>();
            foreach (var it in DrawPlayer.hand.tiles)
            {
                if (CanDiscard(it, out var command))
                {
                    result.Add(command);
                }
            }
            commands = result.ToArray();
            return commands.Length > 0;
        }

        public bool CanDiscard(Tile tile, out Commands.Discard command)
        {
            if (tile == newTileInHand)
            {
                command = new Commands.Discard(this, tile, riichi: false, openRiichi: false);
                return true;
            }

            if (DrawPlayer.Riichi)
            {
                command = default;
                return false;
            }

            if (DrawPlayer.round.game.rule.喰い替え == Rules.喰い替え.なし)
            {
                if (BuiltMeld)
                {
                    var meld = DrawPlayer.hand.melds.Last();
                    if (meld.Is喰い替え(tile))
                    {
                        command = default;
                        return false;
                    }
                }
            }
            if (!DrawPlayer.hand.tiles.Contains(tile))
            {
                command = default;
                return false;
            }

            command = new Commands.Discard(this, tile, riichi: false, openRiichi: false);
            return true;
        }

        public AfterDiscard Discard(Tile tile, bool riichi, bool openRiichi = false)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            DrawPlayer.Discard(tile, riichi, openRiichi);
            if (openDoraAfterDiscard)
            {
                DrawPlayer.round.deadWallTile.OpenDora();
            }
            return new AfterDiscard(DrawPlayer);
        }

        public bool CanTsumo(out Commands.Tsumo command)
        {
            if (!canTsumo)
            {
                command = default;
                return false;
            }

            command = new Commands.Tsumo(this);
            return true;
        }

        public AfterDraw? Tsumo(
            out RoundResult roundResult,
            out Dictionary<Player, CompletedResult> result)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            // 明槓だとここでドラが増えるので点数の確定もここでおこなう
            if (openDoraAfterDiscard)
            {
                Round.deadWallTile.OpenDora();
            }
            var completedHand = CompletedHand;

            return CompletedHand.Execute(
                new Dictionary<Player, CompletedHand>() { { DrawPlayer, completedHand } },
                out roundResult,
                out result);
        }

        public bool CanDeclareClosedQuad(out Commands.DeclareClosedQuad[] commands)
        {
            var result = new List<Commands.DeclareClosedQuad>();
            foreach (var tile in DrawPlayer.hand.tiles.Select(_ => _.type).Distinct())
            {
                if (CanDeclareClosedQuad(tile, out var command))
                {
                    result.Add(command);
                }
            }

            commands = result.ToArray();
            return commands.Length > 0;
        }

        public bool CanDeclareClosedQuad(TileType tile, out Commands.DeclareClosedQuad command)
        {
            // 海底はカンできない
            if (Round.wallTile.tiles.Count == 0)
            {
                command = default;
                return false;
            }
            // 鳴いた直後にカンはできない
            if (BuiltMeld)
            {
                command = default;
                return false;
            }
            // 暗槓は立直後でもできるが、ツモ牌でのみ
            if (DrawPlayer.Riichi)
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

            command = new Commands.DeclareClosedQuad(this, tile);
            return true;
        }

        public BeforeClosedQuad DeclareClosedQuad(TileType tile)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            if (openDoraAfterDiscard)
            {
                DrawPlayer.round.deadWallTile.OpenDora();
            }

            return new BeforeClosedQuad(DrawPlayer, tile);
        }

        public bool CanDeclareAddedOpenQuad(out Commands.DeclareAddedOpenQuad[] commands)
        {
            var result = new List<Commands.DeclareAddedOpenQuad>();
            foreach (var it in DrawPlayer.hand.tiles.Select(_ => _.type).Distinct())
            {
                if (CanDeclareAddedOpenQuad(it, out var command))
                {
                    result.Add(command);
                }
            }
            commands = result.ToArray();
            return commands.Length > 0;
        }

        public bool CanDeclareAddedOpenQuad(TileType tile, out Commands.DeclareAddedOpenQuad command)
        {
            // 海底はカンできない
            if (Round.wallTile.tiles.Count == 0)
            {
                command = default;
                return false;
            }
            // 鳴いた直後にカンはできない
            if (BuiltMeld)
            {
                command = default;
                return false;
            }
            if (!DrawPlayer.CanAddedOpenQuad(tile))
            {
                command = default;
                return false;
            }
            var t = DrawPlayer.hand.tiles.First(_ => _.type == tile);
            command = new Commands.DeclareAddedOpenQuad(this, t);
            return true;
        }
        public BeforeAddedOpenQuad DeclareAddedOpenQuad(Tile tile)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            if (openDoraAfterDiscard)
            {
                DrawPlayer.round.deadWallTile.OpenDora();
            }

            return new BeforeAddedOpenQuad(DrawPlayer, tile);
        }

        public bool Can九種九牌(out Commands.九種九牌 command)
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

            if (DrawPlayer.hand.tiles
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

        public AfterDraw? 九種九牌(out RoundResult roundResult)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            var result = Round.game.AdvanceRoundBy途中流局(out var gameResult);
            roundResult = new RoundResult(gameResult);
            return result;
        }

        public AfterDraw ResetRound(params TileType[]?[]? initialPlayerTilesByCheat)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            return Round.game.StartRound(initialPlayerTilesByCheat);
        }

        public IController DoDefaultAction(out RoundResult? roundResult)
        {
            throw new System.NotImplementedException();
        }

        public ICommand[] ExecutableCommands
        {
            get
            {
                var result = new List<ICommand>();

                if (CanDeclareClosedQuad(out var closedQuads))
                {
                    result.AddRange(closedQuads.Cast<ICommand>());
                }
                if (CanDeclareAddedOpenQuad(out var addedOpenQuads))
                {
                    result.AddRange(addedOpenQuads.Cast<ICommand>());
                }
                if (CanDiscard(out var discards))
                {
                    result.AddRange(discards.Cast<ICommand>());
                }
                if (CanRiichi(out var riichies))
                {
                    result.AddRange(riichies.Cast<ICommand>());
                }
                if (CanOpenRiichi(out var openRiichies))
                {
                    result.AddRange(openRiichies.Cast<ICommand>());
                }
                if (CanTsumo(out var tsumo))
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
            var selector = new CommandSelector(this);
            selector.commands.AddRange(commands);
            return selector.Execute(out executedCommands);
        }
    }
}
