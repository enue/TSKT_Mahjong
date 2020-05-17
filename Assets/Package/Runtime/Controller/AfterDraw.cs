using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class AfterDraw : IController
    {
        public Round Round => DrawPlayer.round;
        public int DrawPlayerIndex => DrawPlayer.index;
        public Player DrawPlayer { get; }

        public bool Consumed { get; private set; }

        public readonly Tile newTileInHand;
        public readonly Hands.Solution handSolution;
        public readonly CompletedHand? tsumo;

        public readonly Dictionary<Player, CompletedHand> rons = new Dictionary<Player, CompletedHand>();
        public readonly bool openDoraAfterDiscard;

        bool 鳴きなし => Round.players.All(_ => _.hand.melds.Count == 0);
        bool 一巡目 => DrawPlayer.discardedTiles.Count == 0;
        bool DrewFromOtherPlayer => newTileInHand == null;

        public AfterDraw(Player player, Tile newTileInHand,
            bool 嶺上,
            bool openDoraAfterDiscard)
        {
            DrawPlayer = player;
            this.newTileInHand = newTileInHand;
            this.openDoraAfterDiscard = openDoraAfterDiscard;

            if (newTileInHand != null)
            {
                handSolution = DrawPlayer.hand.Solve();
                if (handSolution.向聴数 == -1)
                {
                    tsumo = handSolution.ChoiceCompletedHand(DrawPlayer, newTileInHand.type,
                        ronTarget: null,
                        嶺上: 嶺上,
                        海底: Round.wallTile.tiles.Count == 0,
                        河底: false,
                        天和: 鳴きなし && 一巡目 && player.IsDealer,
                        地和: 鳴きなし && 一巡目 && !player.IsDealer,
                        人和: false,
                        槍槓: false);
                }
            }
        }

        public bool CanRiichi(Tile tile)
        {
            if (DrawPlayer.Riichi)
            {
                return false;
            }
            if (Round.game.rule.end.endWhenScoreUnderZero)
            {
                if (DrawPlayer.scoreOwner.score < 1000)
                {
                    return false;
                }
            }
            if (!DrawPlayer.hand.tiles.Contains(tile))
            {
                return false;
            }
            if (DrawPlayer.hand.melds.Count > 0 && DrawPlayer.hand.melds.Any(_ => !_.暗槓))
            {
                return false;
            }
            if (Round.wallTile.tiles.Count == 0)
            {
                return false;
            }
            var solution = DrawPlayer.hand.Clone();
            solution.tiles.Remove(tile);
            var score = solution.Solve();
            return score.向聴数 == 0;
        }

        public bool CanDiscard(Tile tile)
        {
            if (DrawPlayer.Riichi)
            {
                return tile == newTileInHand;
            }
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

        public bool CanTsumo()
        {
            if (handSolution == null)
            {
                return false;
            }
            if (handSolution.向聴数 != -1)
            {
                return false;
            }
            return !tsumo.Value.役無し;
        }

        public AfterDraw Tsumo(
            out RoundResult roundResult,
            out Dictionary<Player, CompletedResult> result)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            return CompletedHand.Execute(
                new Dictionary<Player, CompletedHand>() { { DrawPlayer, tsumo.Value } },
                out roundResult,
                out result);
        }

        public bool CanRon(Player player)
        {
            return false;
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
            throw new System.NotImplementedException();
        }

        public bool CanDeclareClosedQuad(TileType tile)
        {
            // 海底はカンできない
            if (Round.wallTile.tiles.Count == 0)
            {
                return false;
            }
            // 鳴いた直後にカンはできない
            if (DrewFromOtherPlayer)
            {
                return false;
            }
            // 暗槓は立直後でもできる
            return DrawPlayer.CanClosedQuad(tile);
        }
        public BeforeClosedQuad DeclareClosedQuad(TileType tile)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            return new BeforeClosedQuad(DrawPlayer, tile);
        }

        public bool CanDeclareAddedOpenQuad(TileType tile)
        {
            // 海底はカンできない
            if (Round.wallTile.tiles.Count == 0)
            {
                return false;
            }
            // 鳴いた直後にカンはできない
            if (DrewFromOtherPlayer)
            {
                return false;
            }
            return DrawPlayer.CanAddedOpenQuad(tile);
        }
        public BeforeAddedOpenQuad DeclareAddedOpenQuad(Tile tile)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            return new BeforeAddedOpenQuad(DrawPlayer, tile);
        }

        public bool Can九種九牌
        {
            get
            {
                if (!鳴きなし)
                {
                    return false;
                }
                if (!一巡目)
                {
                    return false;
                }
                return DrawPlayer.hand.tiles
                    .Select(_ => _.type)
                    .Where(_ => _.么九牌())
                    .Distinct()
                    .Count() >= 9;
            }
        }

        public AfterDraw 九種九牌(out RoundResult roundResult)
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

        public AfterDraw ResetRound(params TileType[][] initialPlayerTilesByCheat)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            return Round.game.StartRound(initialPlayerTilesByCheat);
        }
    }
}
