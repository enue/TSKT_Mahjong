using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT.Mahjongs
{
    public class AfterDiscard : IController, IRonableController
    {
        public Round Round => DiscardPlayer.round;
        public int DiscardPlayerIndex => DiscardPlayer.index;
        public Player DiscardPlayer { get; }
        public bool Consumed { get; private set; }

        public Dictionary<Player, CompletedHand> PlayerRons { get; } = new Dictionary<Player, CompletedHand>();

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
                        PlayerRons.Add(ronPlayer, completed);
                    }
                }
            }
        }

        static public AfterDiscard FromSerializable(Serializables.AfterDiscard source)
        {
            var round = source.round.Deserialzie();
            var player = round.players[source.discardPlayerIndex];
            return new AfterDiscard(player);
        }

        public Serializables.AfterDiscard ToSerializable()
        {
            return new Serializables.AfterDiscard(this);
        }
        public Serializables.Session SerializeSession()
        {
            return new Serializables.Session(this);
        }

        bool CanRoundContinue
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

        public AfterDraw AdvanceTurn(out RoundResult roundResult)
        {
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            if (CanRoundContinue)
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
                roundResult = null;
                return Round.players[playerIndex].Draw();
            }

            return FinishRound(out roundResult);
        }

        bool ShouldSuspendRound
        {
            get
            {
                if (Round.game.rule.四家立直 == Rules.四家立直.流局)
                {
                    if (四家立直)
                    {
                        return true;
                    }
                }
                if (四開槓)
                {
                    return true;
                }
                if (四風子連打)
                {
                    return true;
                }
                return false;
            }
        }

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

        AfterDraw FinishRound(out RoundResult roundResult)
        {
            if (ShouldSuspendRound)
            {
                var result = Round.game.AdvanceRoundBy途中流局(out var gameResult);
                roundResult = new RoundResult(gameResult);
                return result;
            }
            return FinishRoundAsExhausiveDraw(out roundResult);
        }

        AfterDraw FinishRoundAsExhausiveDraw(out RoundResult roundResult)
        {
            var scoreDiffs = Round.players.ToDictionary(_ => _, _ => 0);
            var states = new Dictionary<Player, ExhausiveDrawType>();

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
                    states.Add(it, (it.hand.Solve().向聴数 == 0)
                        ? ExhausiveDrawType.テンパイ
                        : ExhausiveDrawType.ノーテン);
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
                    var result = Round.game.AdvanceRoundByノーテン流局(out var gameResult);
                    roundResult = new RoundResult(gameResult);
                    roundResult.scoreDiffs = scoreDiffs;
                    roundResult.states = states;
                    return result;
                }
                else if (dealerState == ExhausiveDrawType.流し満貫)
                {
                    var result = Round.game.AdvanceRoundBy親上がり(out var gameResult);
                    roundResult = new RoundResult(gameResult);
                    roundResult.scoreDiffs = scoreDiffs;
                    roundResult.states = states;
                    return result;
                }
                else if (dealerState == ExhausiveDrawType.テンパイ)
                {
                    var result = Round.game.AdvanceRoundByテンパイ流局(out var gameResult);
                    roundResult = new RoundResult(gameResult);
                    roundResult.scoreDiffs = scoreDiffs;
                    roundResult.states = states;
                    return result;
                }
                else
                {
                    throw new System.ArgumentException(dealerState.ToString());
                }
            }
            else
            {
                // 子の流し満貫
                var result = Round.game.AdvanceRoundBy子上がり(out var gameResult);
                roundResult = new RoundResult(gameResult);
                roundResult.scoreDiffs = scoreDiffs;
                roundResult.states = states;
                return result;
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
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

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
            if (Consumed)
            {
                throw new System.Exception("consumed controller");
            }
            Consumed = true;

            foreach (var it in Round.players)
            {
                it.一発 = false;
            }

            var discardPile = DiscardPlayer.discardPile;
            var tile = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            var meld = new Meld();
            player.hand.melds.Add(meld);
            meld.tileFroms.Add((tile, DiscardPlayer.index));

            player.hand.tiles.Remove(対子.Item1);
            meld.tileFroms.Add((対子.Item1, player.index));
            player.hand.tiles.Remove(対子.Item2);
            meld.tileFroms.Add((対子.Item2, player.index));

            return new AfterDraw(player, null, 嶺上: false, openDoraAfterDiscard: false);
        }

        public bool CanChi(Player player, out List<(Tile left, Tile right)> combinations)
        {
            if (player == DiscardPlayer)
            {
                combinations = null;
                return false;
            }

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

            return player.CanChi(DiscardedTile, out combinations);
        }
        public AfterDraw Chi(Player player, (Tile, Tile) 塔子)
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
            var discardPile = DiscardPlayer.discardPile;
            var tile = discardPile[discardPile.Count - 1];
            discardPile.RemoveAt(discardPile.Count - 1);

            var meld = new Meld();
            player.hand.melds.Add(meld);
            meld.tileFroms.Add((tile, DiscardPlayer.index));

            player.hand.tiles.Remove(塔子.Item1);
            meld.tileFroms.Add((塔子.Item1, player.index));
            player.hand.tiles.Remove(塔子.Item2);
            meld.tileFroms.Add((塔子.Item2, player.index));

            return new AfterDraw(player, null, 嶺上: false, openDoraAfterDiscard: false);
        }

        public IController DoDefaultAction(out RoundResult roundResult)
        {
            return AdvanceTurn(out roundResult);
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
                        result.Add(new Commands.Ron(player, this));
                    }
                    if (CanChi(player, out var combinations))
                    {
                        foreach(var combination in combinations)
                        {
                            result.Add(new Commands.Chi(player, this, combination));
                        }
                    }
                    if (CanPon(player, out var pairs))
                    {
                        foreach (var pair in pairs)
                        {
                            result.Add(new Commands.Pon(player, this, pair));
                        }
                    }
                    if (CanOpenQuad(player))
                    {
                        result.Add(new Commands.Kan(player, this));
                    }
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
