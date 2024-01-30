#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs
{
    public interface IController
    {
        局 局 { get; }
        bool Consumed { get; }

        AfterDraw? DoDefaultAction(out RoundResult? roundResult);
        ICommand[] ExecutableCommands { get; }
        ClaimingCommandSet GetExecutableClaimingCommandsBy(Player player);
        DiscardingCommandSet GetExecutableDiscardingCommandsBy(Player player);

        CommandResult ExecuteCommands(out List<ICommand> executedCommands, params ICommand[] commands);
        Serializables.Session SerializeSession();
    }

    public interface IBefore槓 : IController
    {
    }

    public readonly struct DiscardingCommandSet
    {
        readonly Commands.暗槓[]? closedQuads;
        public readonly Commands.暗槓[] 暗槓 => closedQuads ?? System.Array.Empty<Commands.暗槓>();
        readonly Commands.加槓[]? addedOpenQuads;
        public readonly Commands.加槓[] 加槓 => addedOpenQuads ?? System.Array.Empty<Commands.加槓>();
        readonly Commands.Discard[]? discards;
        public readonly Commands.Discard[] Discards => discards ?? System.Array.Empty<Commands.Discard>();
        readonly Commands.Discard[]? riichies;
        public readonly Commands.Discard[] Riichies => riichies ?? System.Array.Empty<Commands.Discard>();
        readonly Commands.Discard[]? openRiichies;
        public readonly Commands.Discard[] OpenRiichies => openRiichies ?? System.Array.Empty<Commands.Discard>();
        public readonly Commands.ツモ上がり? ツモ上がり { get; }
        public readonly Commands.九種九牌? 九種九牌 { get; }

        public DiscardingCommandSet(
            Commands.暗槓[]? 暗槓,
            Commands.加槓[]? 加槓,
            Commands.Discard[]? discards,
            Commands.Discard[]? riichies,
            Commands.Discard[]? openRiichies,
            Commands.ツモ上がり? ツモ上がり,
            Commands.九種九牌? nineTiles)
        {
            this.closedQuads = 暗槓;
            this.addedOpenQuads = 加槓;
            this.discards = discards;
            this.riichies = riichies;
            this.openRiichies = openRiichies;
            this.ツモ上がり = ツモ上がり;
            九種九牌 = nineTiles;
        }

        public bool Empty
        {
            get
            {
                return 暗槓.Length == 0
                    && 加槓.Length == 0
                    && Discards.Length == 0
                    && Riichies.Length == 0
                    && OpenRiichies.Length == 0
                    && !ツモ上がり.HasValue
                    && !九種九牌.HasValue;
            }
        }
        public bool ShouldDiscardLastDrawnTile
        {
            get
            {
                return Discards.Length == 1
                    && 加槓.Length == 0
                    && 暗槓.Length == 0
                    && !九種九牌.HasValue
                    && !ツモ上がり.HasValue
                    && Riichies.Length == 0
                    && OpenRiichies.Length == 0;
            }
        }

        public Commands.CommandPriority MaxPriority
        {
            get
            {
                var result = Commands.CommandPriority.Lowest;
                foreach (var it in 暗槓)
                {
                    if (result < it.Priority)
                    {
                        result = it.Priority;
                    }
                }
                foreach (var it in 加槓)
                {
                    if (result < it.Priority)
                    {
                        result = it.Priority;
                    }
                }
                foreach (var it in Discards)
                {
                    if (result < it.Priority)
                    {
                        result = it.Priority;
                    }
                }
                foreach (var it in Riichies)
                {
                    if (result < it.Priority)
                    {
                        result = it.Priority;
                    }
                }
                foreach (var it in OpenRiichies)
                {
                    if (result < it.Priority)
                    {
                        result = it.Priority;
                    }
                }
                if (ツモ上がり.HasValue && result < ツモ上がり.Value.Priority)
                {
                    result = ツモ上がり.Value.Priority;
                }
                if (九種九牌.HasValue && result < 九種九牌.Value.Priority)
                {
                    result = 九種九牌.Value.Priority;
                }

                return result;
            }
        }
    }
    public readonly struct ClaimingCommandSet
    {
        public readonly Commands.Ron? Ron { get; }
        readonly Commands.Chi[]? chies;
        public readonly Commands.Chi[] Chies => chies ?? System.Array.Empty<Commands.Chi>();
        readonly Commands.Pon[]? pons;
        public readonly Commands.Pon[] Pons => pons ?? System.Array.Empty<Commands.Pon>();
        public readonly Commands.Kan? Kan { get; }
        public ClaimingCommandSet(
            Commands.Ron? ron,
            Commands.Chi[]? chies,
            Commands.Pon[]? pons,
            Commands.Kan? kan)
        {
            Ron = ron;
            this.chies = chies;
            this.pons = pons;
            Kan = kan;
        }

        public Commands.Pon[] DistinctPons
        {
            get
            {
                var dict = new Dictionary<(TileType, bool, TileType, bool, TileType, bool), Commands.Pon>();
                foreach (var it in Pons)
                {
                    dict[it.Key] = it;
                }
                return dict.Values.ToArray();
            }
        }

        public Commands.Chi[] DistinctChies
        {
            get
            {
                var dict = new Dictionary<(TileType, bool, TileType, bool, TileType, bool), Commands.Chi>();
                foreach (var it in Chies)
                {
                    dict[it.Key] = it;
                }
                return dict.Values.ToArray();
            }
        }
        public bool Empty
        {
            get
            {
                return !Ron.HasValue
                    && Chies.Length == 0
                    && Pons.Length == 0
                    && !Kan.HasValue;
            }
        }

        public Commands.CommandPriority MaxPriority
        {
            get
            {
                var result = Commands.CommandPriority.Lowest;
                if (Ron.HasValue && result < Ron.Value.Priority)
                {
                    result = Ron.Value.Priority;
                }
                foreach (var it in Chies)
                {
                    if (result < it.Priority)
                    {
                        result = it.Priority;
                    }
                }
                foreach (var it in Pons)
                {
                    if (result < it.Priority)
                    {
                        result = it.Priority;
                    }
                }
                if (Kan.HasValue && result < Kan.Value.Priority)
                {
                    result = Kan.Value.Priority;
                }
                return result;
            }
        }
    }
}