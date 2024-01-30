#nullable enable
using System.Collections.Generic;
using System.Linq;
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
                    .Cast<Commands.Ron>()
                    .Select(_ => (_.Executor, _.RonResult))
                    .ToArray();
                executedCommands.AddRange(selectedCommands);
                return 和了.Execute(rons);
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

        public readonly 局Result? 局Result;
        public readonly Dictionary<Player, 和了Result>? 和了Results;

        public CommandResult(AfterDiscard? nextController,
            局Result? roundResult = null,
            Dictionary<Player, 和了Result>? completedResults = null)
        {
            this.nextController = nextController;
            afterDiscard = nextController;
            this.局Result = roundResult;
            this.和了Results = completedResults;

            afterDraw = null;
        }
        public CommandResult(AfterDraw? nextController,
            局Result? roundResult = null,
            Dictionary<Player, 和了Result>? completedResults = null)
        {
            this.nextController = nextController;
            afterDraw = nextController;
            this.局Result = roundResult;
            this.和了Results = completedResults;

            afterDiscard = null;
        }
        public CommandResult(IBefore槓? nextController,
            局Result? roundResult = null,
            Dictionary<Player, 和了Result>? completedResults = null)
        {
            this.nextController = nextController;

            this.局Result = roundResult;
            this.和了Results = completedResults;

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
