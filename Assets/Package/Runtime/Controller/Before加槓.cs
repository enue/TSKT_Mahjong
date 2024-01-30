#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs
{
    /// <summary>
    /// 加槓宣言時。槍槓ができる
    /// </summary>
    public class Before加槓 : IBefore槓
    {
        public 局 局 => DeclarePlayer.局;
        public bool Consumed { get; private set; }
        public PlayerIndex DeclarePlayerIndex => DeclarePlayer.index;
        public Player DeclarePlayer { get; }


        public readonly Tile tile;
        Dictionary<Player, 和了> PlayerRons { get; } = new Dictionary<Player, 和了>();

        public Before加槓(Player declarePlayer, Tile tile)
        {
            DeclarePlayer = declarePlayer;
            this.tile = tile;

            foreach (var ronPlayer in 局.players)
            {
                if (ronPlayer == DeclarePlayer)
                {
                    continue;
                }
                if (ronPlayer.フリテン)
                {
                    continue;
                }
                var hand = ronPlayer.手牌.Clone();
                hand.tiles.Add(tile);
                var solution = hand.Solve();
                if (solution.向聴数 > -1)
                {
                    continue;
                }
                var completed = solution.Choice和了(ronPlayer, tile.type,
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

        public static Before加槓 FromSerializable(in Serializables.Before加槓 source)
        {
            var round = source.局.Deserialize();
            var player = round.players[(int)source.declarePlayerIndex];
            var tile = round.壁牌.allTiles[source.tile];
            return new Before加槓(player, tile);
        }
        public Serializables.Before加槓 ToSerializable()
        {
            return new Serializables.Before加槓(this);
        }
        public Serializables.Session SerializeSession()
        {
            return new Serializables.Session(this);
        }

        bool CanRon(out Commands.Ron[] commands)
        {
            commands = PlayerRons.Select(_ => new Commands.Ron(_.Key, this, _.Value)).ToArray();
            return commands.Length > 0;
        }

        bool CanRon(Player player, out Commands.Ron command)
        {
            if (!PlayerRons.TryGetValue(player, out var hand))
            {
                command = default;
                return false;
            }
            command = new Commands.Ron(player, this, hand);
            return true;
        }

        public AfterDraw? DoDefaultAction(out 局Result? roundResult)
        {
            roundResult = null;
            return 局.Execute加槓(DeclarePlayer, tile);
        }

        public void GetExecutableCommands(out Commands.Ron[] rons)
        {
            CanRon(out rons);
        }

        public DiscardingCommandSet GetExecutableDiscardingCommandsBy(Player player)
        {
            return default;
        }
        public ClaimingCommandSet GetExecutableClaimingCommandsBy(Player player)
        {
            if (CanRon(player, out var ron))
            {
                return new ClaimingCommandSet(ron: ron, null, null, null);
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

                if (CanRon(out var rons))
                {
                    result.AddRange(rons.Cast<ICommand>());
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
