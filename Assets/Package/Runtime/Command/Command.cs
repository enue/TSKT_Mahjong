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

            var maxPriority = MaxPriority;
            var selectedCommands = commands.Where(_ => _.Priority == maxPriority);

            // ダブロン、トリロンの場合
            if (maxPriority == Commands.CommandPriority.Ron
                && selectedCommands.Count() > 1)
            {
                var ronExecutor = new List<Player>();
                foreach (var it in selectedCommands)
                {
                    executedCommands.Add(it);
                    ronExecutor.Add(it.Executor);
                }

                return ((IRonableController)origin).Ron(ronExecutor.ToArray());
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
        public readonly IBeforeQuad? beforeQuad;

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
            beforeQuad = null;
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
            beforeQuad = null;
        }
        public CommandResult(IBeforeQuad? nextController,
            RoundResult? roundResult = null,
            Dictionary<Player, CompletedResult>? completedResults = null)
        {
            this.nextController = nextController;
            beforeQuad = nextController;

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
