#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs
{
    public interface IController
    {
        Round Round { get; }
        bool Consumed { get; }

        AfterDraw? DoDefaultAction(out RoundResult? roundResult);
        ICommand[] ExecutableCommands { get; }
        CommandSet GetExecutableCommandsBy(Player player);

        CommandResult ExecuteCommands(out List<ICommand> executedCommands, params ICommand[] commands);
        Serializables.Session SerializeSession();
    }

    public interface IRonableController : IController
    {
        bool CanRon(out Commands.Ron[] command);
        bool CanRon(Player player, out Commands.Ron command);
        AfterDraw? Ron(out RoundResult roundResult, out Dictionary<Player, CompletedResult> result, params Player[] players);
        Dictionary<Player, CompletedHand> PlayerRons { get; }
    }

    public interface IBeforeQuad : IController, IRonableController
    {
        Player DeclarePlayer { get; }
        AfterDraw BuildQuad();
    }

    public readonly struct CommandSet
    {
        public readonly Commands.Ron? Ron { get; }
        readonly Commands.Chi[]? chies;
        public readonly Commands.Chi[] Chies => chies ?? System.Array.Empty<Commands.Chi>();
        readonly Commands.Pon[]? pons;
        public readonly Commands.Pon[] Pons => pons ?? System.Array.Empty<Commands.Pon>();
        public readonly Commands.Kan? Kan { get; }
        readonly Commands.DeclareClosedQuad[]? closedQuads;
        public readonly Commands.DeclareClosedQuad[] ClosedQuads => closedQuads ?? System.Array.Empty<Commands.DeclareClosedQuad>();
        readonly Commands.DeclareAddedOpenQuad[]? addedOpenQuads;
        public readonly Commands.DeclareAddedOpenQuad[] AddedOpenQuads => addedOpenQuads ?? System.Array.Empty<Commands.DeclareAddedOpenQuad>();
        readonly Commands.Discard[]? discards;
        public readonly Commands.Discard[] Discards => discards ?? System.Array.Empty<Commands.Discard>();
        readonly Commands.Discard[]? riichies;
        public readonly Commands.Discard[] Riichies => riichies ?? System.Array.Empty<Commands.Discard>();
        readonly Commands.Discard[]? openRiichies;
        public readonly Commands.Discard[] OpenRiichies => openRiichies ?? System.Array.Empty<Commands.Discard>();
        public readonly Commands.Tsumo? Tsumo { get; }
        public readonly Commands.九種九牌? NineTiles { get; }

        public CommandSet(
            Commands.Ron? ron,
            Commands.Chi[]? chies,
            Commands.Pon[]? pons,
            Commands.Kan? kan,
            Commands.DeclareClosedQuad[]? closedQuads,
            Commands.DeclareAddedOpenQuad[]? addedOpenQuads,
            Commands.Discard[]? discards,
            Commands.Discard[]? riichies,
            Commands.Discard[]? openRiichies,
            Commands.Tsumo? tsumo,
            Commands.九種九牌? nineTiles)
        {
            Ron = ron;
            this.chies = chies;
            this.pons = pons;
            Kan = kan;
            this.closedQuads = closedQuads;
            this.addedOpenQuads = addedOpenQuads;
            this.discards = discards;
            this.riichies = riichies;
            this.openRiichies = openRiichies;
            Tsumo = tsumo;
            NineTiles = nineTiles;
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
                    && !Kan.HasValue
                    && ClosedQuads.Length == 0
                    && AddedOpenQuads.Length == 0
                    && Discards.Length == 0
                    && Riichies.Length == 0
                    && OpenRiichies.Length == 0
                    && !Tsumo.HasValue
                    && !NineTiles.HasValue;
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
                foreach (var it in ClosedQuads)
                {
                    if (result < it.Priority)
                    {
                        result = it.Priority;
                    }
                }
                foreach (var it in AddedOpenQuads)
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
                if (Tsumo.HasValue && result < Tsumo.Value.Priority)
                {
                    result = Tsumo.Value.Priority;
                }
                if (NineTiles.HasValue && result < NineTiles.Value.Priority)
                {
                    result = NineTiles.Value.Priority;
                }

                return result;
            }
        }
    }
}
