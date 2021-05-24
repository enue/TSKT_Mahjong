using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable

namespace TSKT.Mahjongs
{
    public interface ICommand
    {
        Commands.CommandPriority Priority { get; }
        CommandResult? TryExecute();
        Player Executor { get; }
    }

    public interface ICommand<T> : ICommand
        where T : IController
    {
        T Controller { get; }
    }

    public class CommandSelector
    {
        public readonly IController origin;
        public readonly List<ICommand> commands = new List<ICommand>();
        public Commands.CommandPriority MaxPriority => commands.Count == 0 ? Commands.CommandPriority.Lowest : commands.Max(_ => _.Priority);

        public CommandSelector(IController origin)
        {
            this.origin = origin;
        }
        public CommandResult Execute(out List<ICommand> executedCommands)
        {
            executedCommands = new List<ICommand>();

            if (commands.Count == 0)
            {
                var nextController = origin.DoDefaultAction(out var roundResult);
                return new CommandResult(nextController, roundResult);
            }

            var ronExecutor = new List<Player>();
            var maxPriority = MaxPriority;
            foreach (var it in commands.Where(_ => _.Priority == maxPriority))
            {
                executedCommands.Add(it);
                var commandResult = it.TryExecute();
                if (commandResult != null)
                {
                    Debug.Assert(ronExecutor.Count == 0);
                    return commandResult;
                }

                ronExecutor.Add(it.Executor);
            }

            Debug.Assert(ronExecutor.Count > 0);

            {
                var nextController = ((IRonableController)origin).Ron(out var roundResult, out var completedResults, ronExecutor.ToArray());
                return new CommandResult(nextController, roundResult, completedResults);
            }
        }
    }

    public class CommandResult
    {
        public readonly IController? nextController;
        public readonly RoundResult? roundResult;
        public readonly Dictionary<Player, CompletedResult>? completedResults;

        public CommandResult(IController? nextController,
            RoundResult? roundResult = null,
            Dictionary<Player, CompletedResult>? completedResults = null)
        {
            this.nextController = nextController;
            this.roundResult = roundResult;
            this.completedResults = completedResults;
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
