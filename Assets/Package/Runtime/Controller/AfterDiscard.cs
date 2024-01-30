#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs
{
    public class AfterDiscard : IController
    {
        public 局 局 => DiscardPlayer.局;
        public bool Consumed { get; private set; }
        public PlayerIndex DiscardPlayerIndex => DiscardPlayer.index;
        public Player DiscardPlayer { get; }

        Dictionary<Player, 和了> PlayerRons { get; } = new Dictionary<Player, 和了>();

        public Tile 捨て牌 => 局.players[(int)DiscardPlayerIndex].河.Last();

        public AfterDiscard(Player discardPlayer)
        {
            DiscardPlayer = discardPlayer;

            var 鳴きなし = 局.players.All(_ => _.手牌.副露.Count == 0);

            foreach (var ronPlayer in 局.players)
            {
                if (ronPlayer == DiscardPlayer)
                {
                    continue;
                }
                if (ronPlayer.フリテン)
                {
                    continue;
                }
                var hand = ronPlayer.手牌.Clone();
                hand.tiles.Add(捨て牌);
                var solution = hand.Solve();
                if (solution.向聴数 > -1)
                {
                    continue;
                }
                var 一巡目 = ronPlayer.捨て牌.Count == 0;
                var completed = solution.Choice和了(ronPlayer, 捨て牌.type,
                    ronTarget: DiscardPlayer,
                    嶺上: false,
                    海底: false,
                    河底: 局.壁牌.tiles.Count == 0,

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

        static public AfterDiscard FromSerializable(in Serializables.AfterDiscard source)
        {
            var round = source.局.Deserialize();
            var player = round.players[(int)source.discardPlayerIndex];
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
                return 局.壁牌.tiles.Count > 0;
            }
        }

        public void TryAttachFuriten()
        {
            foreach (var player in 局.players)
            {
                if (player == DiscardPlayer)
                {
                    continue;
                }
                player.TryAttachFuritenByOtherPlayers(捨て牌);
            }
        }


        bool ShouldSuspendRound
        {
            get
            {
                if (局.game.rule.四家立直 == Rules.四家立直.流局)
                {
                    if (四家立直)
                    {
                        return true;
                    }
                }
                if (局.game.rule.四槓流れ == Rules.四槓流れ.流局)
                {
                    if (四開槓)
                    {
                        return true;
                    }
                }
                if (四風子連打)
                {
                    return true;
                }
                return false;
            }
        }

        bool 四家立直 => 局.players.All(_ => _.リーチ);
        bool 四開槓
        {
            get
            {
                // 一人がカンを四回している場合は四槓子テンパイとなり流れない
                if (局.players.Any(_ => _.手牌.副露.Count(x => x.槓子) == 4))
                {
                    return false;
                }
                return 局.CountKan == 4;
            }
        }

        bool 四風子連打
        {
            get
            {
                Tile? tile = null;
                foreach (var it in 局.players)
                {
                    if (it.手牌.副露.Count > 0)
                    {
                        return false;
                    }
                    if (it.捨て牌.Count != 1)
                    {
                        return false;
                    }
                    var discardedTile = it.捨て牌[0];
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

        AfterDraw? AdvanceTurn(out 局Result? 局Result)
        {
            TryAttachFuriten();

            if (CanRoundContinue)
            {
                var playerIndex = ((int)DiscardPlayerIndex + 1) % 局.players.Length;
                局Result = null;
                return 局.players[playerIndex].Draw();
            }

            if (ShouldSuspendRound)
            {
                var result = 局.game.Advance局By途中流局(out var gameResult);
                局Result = new 局Result(gameResult);
                return result;
            }

            var scoreDiffs = 局.players.ToDictionary(_ => _, _ => 0);
            var states = new Dictionary<Player, ExhausiveDrawType>();

            var 流し満貫 = 局.players
                .Where(_ => _.捨て牌.Count == _.河.Count && _.捨て牌.All(x => x.type.么九牌()))
                .ToArray();
            if (流し満貫.Length > 0)
            {
                foreach (var it in 流し満貫)
                {
                    states.Add(it, ExhausiveDrawType.流し満貫);
                    if (it.Is親)
                    {
                        foreach (var player in 局.players)
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
                        foreach (var player in 局.players)
                        {
                            if (player != it)
                            {
                                scoreDiffs[player] -= player.Is親 ? 4000 : 2000;
                            }
                        }
                        scoreDiffs[it] += 8000;
                    }
                }
            }
            else
            {
                foreach (var it in 局.players)
                {
                    states.Add(it, (it.手牌.向聴数IsLessThanOrEqual(0))
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
                it.Key.Score += it.Value;
            }

            if (states.TryGetValue(局.親, out var dealerState))
            {
                if (dealerState == ExhausiveDrawType.ノーテン)
                {
                    var result = 局.game.Advance局Byノーテン流局(out var gameResult);
                    局Result = new 局Result(gameResult, scoreDiffs, states);
                    return result;
                }
                else if (dealerState == ExhausiveDrawType.流し満貫)
                {
                    var result = 局.game.Advance局By親上がり(out var gameResult);
                    局Result = new 局Result(gameResult, scoreDiffs, states);
                    return result;
                }
                else if (dealerState == ExhausiveDrawType.テンパイ)
                {
                    var result = 局.game.Advance局Byテンパイ流局(out var gameResult);
                    局Result = new 局Result(gameResult, scoreDiffs, states);
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
                var result = 局.game.Advance局By子上がり(out var gameResult);
                局Result = new 局Result(gameResult, scoreDiffs, states);
                return result;
            }
        }

        bool CanRon(out Commands.Ron[] commands)
        {
            commands = PlayerRons.Select(_ => new Commands.Ron(_.Key, this, _.Value)).ToArray();
            return commands.Length > 0;
        }

        bool CanRon(Player player, out Commands.Ron command)
        {
            if (PlayerRons.TryGetValue(player, out var hand))
            {
                command = new Commands.Ron(player, this, hand);
                return true;
            }
            command = default;
            return false;
        }

        bool Can大明槓(out Commands.Kan[] commands)
        {
            var result = new List<Commands.Kan>();
            foreach (var player in 局.players)
            {
                if (Can大明槓(player, out var command))
                {
                    result.Add(command);
                }
            }
            commands = result.ToArray();
            return commands.Length > 0;
        }

        bool Can大明槓(Player player, out Commands.Kan command)
        {
            if (player == DiscardPlayer)
            {
                command = default;
                return false;
            }
            // 河底はカンできない
            if (局.壁牌.tiles.Count == 0)
            {
                command = default;
                return false;
            }
            if (!player.Can大明槓(捨て牌.type))
            {
                command = default;
                return false;
            }

            command = new Commands.Kan(player, this);
            return true;
        }

        bool CanPon(out Commands.Pon[] commands)
        {
            var result = new List<Commands.Pon>();
            foreach (var player in 局.players)
            {
                if (CanPon(player, out var command))
                {
                    result.AddRange(command);
                }
            }
            commands = result.ToArray();
            return commands.Length > 0;
        }

        bool CanPon(Player player, out Commands.Pon[] commands)
        {
            if (player == DiscardPlayer)
            {
                commands = System.Array.Empty<Commands.Pon>();
                return false;
            }
            // 河底はポンできない
            if (局.壁牌.tiles.Count == 0)
            {
                commands = System.Array.Empty<Commands.Pon>();
                return false;
            }
            if (!player.CanPon(捨て牌.type, out var combinations))
            {
                commands = System.Array.Empty<Commands.Pon>();
                return false;
            }
            commands = combinations.Select(_ => new Commands.Pon(player, this, _)).ToArray();
            return true;
        }

        bool CanChi(out Commands.Chi[] commands)
        {
            var result = new List<Commands.Chi>();
            foreach (var player in 局.players)
            {
                if (CanChi(player, out var command))
                {
                    result.AddRange(command);
                }
            }
            commands = result.ToArray();
            return commands.Length > 0;
        }

        bool CanChi(Player player, out Commands.Chi[] commands)
        {
            if (player == DiscardPlayer)
            {
                commands = System.Array.Empty<Commands.Chi>();
                return false;
            }

            // 河底はチーできない
            if (局.壁牌.tiles.Count == 0)
            {
                commands = System.Array.Empty<Commands.Chi>();
                return false;
            }
            if (player.GetRelativePlayer(DiscardPlayer) != RelativePlayer.上家)
            {
                commands = System.Array.Empty<Commands.Chi>();
                return false;
            }

            if (!player.CanChi(捨て牌, out var combinations))
            {
                commands = System.Array.Empty<Commands.Chi>();
                return false;
            }

            commands = combinations.Select(_ => new Commands.Chi(player, this, _)).ToArray();
            return true;
        }

        public AfterDraw? DoDefaultAction(out 局Result? roundResult)
        {
            return AdvanceTurn(out roundResult);
        }
        public ClaimingCommandSet GetExecutableClaimingCommandsBy(Player player)
        {
            Commands.Ron? ron;
            if (CanRon(player, out var _ron))
            {
                ron = _ron;
            }
            else
            {
                ron = null;
            }
            CanChi(player, out var chies);
            CanPon(player, out var pons);
            Commands.Kan? kan;
            if (Can大明槓(player, out var _kan))
            {
                kan = _kan;
            }
            else
            {
                kan = null;
            }

            return new ClaimingCommandSet(ron: ron, chies: chies, pons: pons, kan: kan);
        }
        public DiscardingCommandSet GetExecutableDiscardingCommandsBy(Player player)
        {
            return default;
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
                if (CanChi(out var chies))
                {
                    result.AddRange(chies.Cast<ICommand>());
                }
                if (CanPon(out var pons))
                {
                    result.AddRange(pons.Cast<ICommand>());
                }
                if (Can大明槓(out var kans))
                {
                    result.AddRange(kans.Cast<ICommand>());
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
