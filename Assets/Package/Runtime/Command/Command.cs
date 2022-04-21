#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TSKT.Mahjongs.Rounds;

namespace TSKT.Mahjongs
{
    public interface ICommand
    {
        Commands.CommandPriority Priority { get; }
        CommandResult Execute();
        Player Executor { get; }
    }

    public interface ICommand<T> : ICommand
        where T : IController
    {
        T Controller { get; }
    }

    public readonly struct CommandSelector
    {
        public readonly IController origin;

        public CommandSelector(IController origin)
        {
            this.origin = origin;
        }
        public CommandResult Execute(out List<ICommand> executedCommands, params ICommand[] commands)
        {
            executedCommands = new List<ICommand>();

            if (commands.Length == 0)
            {
                var nextController = origin.DoDefaultAction(out var roundResult);
                return new CommandResult(nextController, roundResult);
            }

            var maxPriority = commands.Length == 0 ? Commands.CommandPriority.Lowest : commands.Max(_ => _.Priority);
            var selectedCommands = commands.Where(_ => _.Priority == maxPriority);

            // ダブロン、トリロンに対応する
            if (maxPriority == Commands.CommandPriority.Ron)
            {
                var rons = selectedCommands
                    .OfType<Commands.Ron>()
                    .Select(_ => (_.Executor, _.RonResult))
                    .ToArray();
                return CompletedHand.Execute(rons);
            }

            var selectedCommand = selectedCommands.First();
            executedCommands.Add(selectedCommand);
            return selectedCommand.Execute();
        }
    }

    public readonly struct CommandResult
    {
        public readonly IController? nextController;

        public readonly AfterDiscard? afterDiscard;
        public readonly AfterDraw? afterDraw;

        public readonly RoundResult? roundResult;
        public readonly Dictionary<Player, CompletedResult>? completedResults;

        public CommandResult(AfterDiscard? nextController,
            RoundResult? roundResult = null,
            Dictionary<Player, CompletedResult>? completedResults = null)
        {
            this.nextController = nextController;
            afterDiscard = nextController;
            this.roundResult = roundResult;
            this.completedResults = completedResults;

            afterDraw = null;
        }
        public CommandResult(AfterDraw? nextController,
            RoundResult? roundResult = null,
            Dictionary<Player, CompletedResult>? completedResults = null)
        {
            this.nextController = nextController;
            afterDraw = nextController;
            this.roundResult = roundResult;
            this.completedResults = completedResults;

            afterDiscard = null;
        }
        public CommandResult(IBeforeQuad? nextController,
            RoundResult? roundResult = null,
            Dictionary<Player, CompletedResult>? completedResults = null)
        {
            this.nextController = nextController;

            this.roundResult = roundResult;
            this.completedResults = completedResults;

            afterDiscard = null;
            afterDraw = null;
        }
    }
}
namespace TSKT.Mahjongs.Commands
{
    public enum CommandPriority
    {
        Lowest,
        Chi,
        Pon,
        Ron,
        Tsumo,
    }
}
