using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class AfterDiscard : IController
    {
        public Round Round => DiscardPlayer.round;
        public int DiscardPlayerIndex => DiscardPlayer.index;
        public Player DiscardPlayer { get; }

        public readonly Dictionary<Player, CompletedHand> rons = new Dictionary<Player, CompletedHand>();

        public Tile DiscardedTile => Round.players[DiscardPlayerIndex].discardPile.Last();

        public AfterDiscard(Player discardPlayer)
        {
            DiscardPlayer = discardPlayer;

            var 鳴きなし = Round.players.All(_ => _.hand.melds.Count == 0);

            foreach (var ronPlayer in Round.players)
            {
                if (ronPlayer == DiscardPlayer)
                {
                    continue;
                }
                if (ronPlayer.フリテン)
                {
                    continue;
                }
                if (ronPlayer.discardedTiles.Any(_ => _.type == DiscardedTile.type))
                {
                    // フリテン
                    continue;
                }
                var hand = ronPlayer.hand.Clone();
                hand.tiles.Add(DiscardedTile);
                var solution = hand.Solve();
                if (solution.向聴数 == -1)
                {
                    var 一巡目 = ronPlayer.discardedTiles.Count == 0;
                    var completed = solution.ChoiceCompletedHand(ronPlayer, DiscardedTile.type,
                        ronTarget: DiscardPlayer,
                        嶺上: false,
                        海底: false,
                        河底: Round.wallTile.tiles.Count == 0,

                        天和: false,
                        地和: false,
                        人和: 鳴きなし && 一巡目,
                        槍槓: false);
                    if (!completed.役無し)
                    {
                        rons.Add(ronPlayer, completed);
                    }
                }
            }
        }

        public bool CanAdvanceTurn
        {
            get
            {
                if (ShouldSuspendRound)
                {
                    return false;
                }
                return Round.wallTile.tiles.Count > 0;
            }
        }

        public AfterDraw AdvanceTurn()
        {
            // フリテン判定
            foreach (var player in Round.players)
            {
                if (player.フリテン)
                {
                    continue;
                }
                var hand = player.hand.Clone();
                hand.tiles.Add(DiscardedTile);
                if (hand.Solve().向聴数 == -1)
                {
                    player.フリテン = true;
                }
            }

            var playerIndex = (DiscardPlayerIndex + 1) % Round.players.Length;
            return Round.players[playerIndex].Draw();
        }

        // TODO : 九種九牌
        // TODO : 三家和
        bool ShouldSuspendRound => 四家立直 || 四開槓 || 四風子連打;
        bool 四家立直 => Round.players.All(_ => _.Riichi);
        bool 四開槓
        {
            get
            {
                // 一人がカンを四回している場合は四槓子テンパイとなり流れない
                if (Round.players.Any(_ => _.hand.melds.Count(x => x.槓子) == 4))
                {
                    return false;
                }
                return Round.CountKan == 4;
            }
        }

        bool 四風子連打
        {
            get
            {
                Tile tile = null;
                foreach (var it in Round.players)
                {
                    if (it.hand.melds.Count > 0)
                    {
                        return false;
                    }
                    if (it.discardedTiles.Count != 1)
                    {
                        return false;
                    }
                    var discardedTile = it.discardedTiles[0];
                    if (!discardedTile.type.風牌())
                    {
                        return false;
                    }
                    if (tile != null && tile.type != discardedTile.type)
                    {
                        return false;
                    }
                    tile = discardedTile;
                }
                return true;
            }
        }

        public GameResult FinishRound(
            out Dictionary<Player, int> scoreDiffs,
            out Dictionary<Player, ExhausiveDrawType> states)
        {
            if (ShouldSuspendRound)
            {
                scoreDiffs = Round.players.ToDictionary(_ => _, _ => 0);
                states = null;
                return Round.game.AdvanceRoundByテンパイ流局();
            }
            return FinishRoundAsExhausiveDraw(out scoreDiffs, out states);
        }

        GameResult FinishRoundAsExhausiveDraw(
            out Dictionary<Player, int> scoreDiffs,
            out Dictionary<Player, ExhausiveDrawType> states)
        {
            scoreDiffs = Round.players.ToDictionary(_ => _, _ => 0);
            states = new Dictionary<Player, ExhausiveDrawType>();

            var 流し満貫 = Round.players
                .Where(_ => _.discardedTiles.Count == _.discardPile.Count && _.discardedTiles.All(x => x.type.么九牌()))
                .ToArray();
            if (流し満貫.Length > 0)
            {
                foreach (var it in 流し満貫)
                {
                    states.Add(it, ExhausiveDrawType.流し満貫);
                    if (it.IsDealer)
                    {
                        foreach (var player in Round.players)
                        {
                            if (player != it)
                            {
                                scoreDiffs[player] -= 4000;
                            }
                        }
                        scoreDiffs[it] += 12000;
                    }
                    else
                    {
                        foreach (var player in Round.players)
                        {
                            if (player != it)
                            {
                                scoreDiffs[player] -= player.IsDealer ? 4000 : 2000;
                            }
                        }
                        scoreDiffs[it] += 8000;
                    }
                }
            }
            else
            {
                foreach (var it in Round.players)
                {
                    states.Add(it, (it.hand.Solve().向聴数 == 0) ? ExhausiveDrawType.テンパイ : ExhausiveDrawType.ノーテン);
                }
                var getterCount = states.Count(_ => _.Value == ExhausiveDrawType.テンパイ);

                if (getterCount > 0 && getterCount < 4)
                {
                    foreach (var it in states)
                    {
                        if (it.Value == ExhausiveDrawType.テンパイ)
                        {
                            scoreDiffs[it.Key] += 3000 / getterCount;
                        }
                        else
                        {
                            scoreDiffs[it.Key] -= 3000 / (4 - getterCount);
                        }
                    }
                }
            }

            foreach (var it in scoreDiffs)
            {
                it.Key.scoreOwner.score += it.Value;
            }

            if (states.TryGetValue(Round.Dealer, out var dealerState))
            {
                if (dealerState == ExhausiveDrawType.ノーテン)
                {
                    return Round.game.AdvanceRoundByノーテン流局();
                }
                else if (dealerState == ExhausiveDrawType.流し満貫)
                {
                    return Round.game.AdvanceRoundBy親上がり();
                }
                else if (dealerState == ExhausiveDrawType.テンパイ)
                {
                    return Round.game.AdvanceRoundByテンパイ流局();
                }
                else
                {
                    throw new System.ArgumentException(dealerState.ToString());
                }
            }
            else
            {
                // 子の流し満貫
                return Round.game.AdvanceRoundBy子上がり();
            }
        }

        public bool CanRon(Player player)
        {
            return rons.ContainsKey(player);
        }

        public bool CanOpenQuad(Player player)
        {
            if (player == DiscardPlayer)
            {
                return false;
            }
            // 河底はカンできない
            if (Round.wallTile.tiles.Count == 0)
            {
                return false;
            }
            return player.CanOpenQuad(DiscardedTile.type);
        }
        public AfterDraw OpenQuad(Player player)
        {
            return Round.ExecuteOpenQuad(player, DiscardPlayer);
        }

        public bool CanPon(Player player, out List<(Tile left, Tile right)> combinations)
        {
            if (player == DiscardPlayer)
            {
                combinations = null;
                return false;
            }
            // 河底はポンできない
            if (Round.wallTile.tiles.Count == 0)
            {
                combinations = null;
                return false;
            }
            return player.CanPon(DiscardedTile.type, out combinations);
        }
        public AfterDraw Pon(Player player, (Tile, Tile) 対子)
        {
            foreach (var it in Round.players)
            {
                it.一発 = false;
            }

            var discardPile = DiscardPlayer.discardPile;
            var tile = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            var meld = new Meld();
            player.hand.melds.Add(meld);
            meld.tileFroms.Add((tile, DiscardPlayer));

            player.hand.tiles.Remove(対子.Item1);
            meld.tileFroms.Add((対子.Item1, player));
            player.hand.tiles.Remove(対子.Item2);
            meld.tileFroms.Add((対子.Item2, player));

            return new AfterDraw(player, null, 嶺上: false, openDoraAfterDiscard: false);
        }

        public bool CanChi(Player player, out List<(Tile left, Tile right)> combinations)
        {
            // 河底はチーできない
            if (Round.wallTile.tiles.Count == 0)
            {
                combinations = null;
                return false;
            }
            if (player.GetRelativePlayer(DiscardPlayer) != RelativePlayer.上家)
            {
                combinations = null;
                return false;
            }

            return player.CanChi(DiscardedTile.type, out combinations);
        }
        public AfterDraw Chi(Player player, (Tile, Tile) 塔子)
        {
            foreach (var it in Round.players)
            {
                it.一発 = false;
            }
            var discardPile = DiscardPlayer.discardPile;
            var tile = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            var meld = new Meld();
            player.hand.melds.Add(meld);
            meld.tileFroms.Add((tile, DiscardPlayer));

            player.hand.tiles.Remove(塔子.Item1);
            meld.tileFroms.Add((塔子.Item1, player));
            player.hand.tiles.Remove(塔子.Item2);
            meld.tileFroms.Add((塔子.Item2, player));

            return new AfterDraw(player, null, 嶺上: false, openDoraAfterDiscard: false);
        }
    }
}
