using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TSKT;
using System.Linq;

namespace TSKT.Mahjongs
{
    public enum CommandPriority
    {
        Lowest,
        Chi,
        Pon,
        Ron,
        Tsumo,
    }

    public interface ICommand
    {
        CommandPriority Priority { get; }
        CommandResult TryExecute();
    }

    public class CommandSelector
    {
        public readonly IController origin;
        public readonly List<ICommand> commands = new List<ICommand>();
        public CommandPriority MaxPriority => commands.Count == 0 ? CommandPriority.Lowest : commands.Max(_ => _.Priority);

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

            var rons = new List<Player>();
            var maxPriority = MaxPriority;
            foreach (var it in commands.Where(_ => _.Priority == maxPriority))
            {
                executedCommands.Add(it);
                var commandResult = it.TryExecute();
                if (commandResult != null)
                {
                    Debug.Assert(rons.Count == 0);
                    return commandResult;
                }

                if (it is Commands.Ron ron)
                {
                    executedCommands.Add(it);
                    rons.Add(ron.player);
                }
                else if (it is Commands.槍槓 槍槓)
                {
                    executedCommands.Add(it);
                    rons.Add(槍槓.player);
                }
                else
                {
                    throw new System.ArgumentException(it.ToString());
                }
            }

            Debug.Assert(rons.Count > 0);

            {
                var nextController = origin.Ron(out var roundResult, out var completedResults, rons.ToArray());
                return new CommandResult(nextController, roundResult, completedResults);
            }
        }
    }

    public class CommandResult
    {
        public readonly IController nextController;
        public readonly RoundResult roundResult;
        public readonly Dictionary<Mahjongs.Player, CompletedResult> completedResults;

        public CommandResult(IController nextController,
            RoundResult roundResult = null,
            Dictionary<Mahjongs.Player, CompletedResult> completedResults = null)
        {
            this.nextController = nextController;
            this.roundResult = roundResult;
            this.completedResults = completedResults;
        }
    }
}
