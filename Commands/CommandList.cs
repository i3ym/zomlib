using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Zomlib.Commands;

public class CommandList : IEnumerable<Command>
{
    public int Count => CommandsList.Count;
    readonly List<Command> CommandsList = new();
    readonly Dictionary<string, List<Command>> CommandsByFirstWord = new();


    public OperationResult<string?> TryExecute(string text, MessageInfo info, out Command cmd)
    {
        if (!Get(text.ToLowerInvariant(), out cmd, out string[]? parameters)) return OperationResult.Err();

        var exec = cmd.Parameters.Execute(info, parameters);
        return EStringFromCommandResult(exec, cmd);
    }

    public static OperationResult<string?> EStringFromCommandResult(in CommandResult result, in Command cmd)
    {
        if (result.Status == CommandResult.Result.ParameterError)
            return ParametersBase.ErrorFromParameter(cmd, cmd.Parameters.Parameters[result.ErrorParameterIndex], result.Message);
        else if (result.Status == CommandResult.Result.NotEnoughParameters)
        {
            var ret = "Использование: " + cmd.Names[0] + " " + cmd.ParametersString;
            if (result.ErrorParameterIndex != 0)
                ret += " (пропущен параметр " + cmd.Parameters.Parameters[result.ErrorParameterIndex].Name + ")";

            return OperationResult.Err( ret);
        }

        return result.Message.AsOpResult();
    }

    public bool Get(string text, [MaybeNullWhen(false)] out Command cmd, [MaybeNullWhen(false)] out string[] parameters)
    {
        if (!GetByCommandString(text.ToLowerInvariant(), out cmd, out string? foundname))
        {
            parameters = null;
            return false;
        }

        parameters = text.Substring(foundname.Length).TrimStart().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return true;
    }
    public bool GetByCommandString(string cmd, [MaybeNullWhen(false)] out Command foundcommand, [MaybeNullWhen(false)] out string foundname)
    {
        var index = cmd.IndexOf(' ');
        var firstWord = index < 0 ? cmd : cmd.Substring(0, index);

        if (CommandsByFirstWord.TryGetValue(firstWord, out var cmds))
            foreach (var command in cmds)
            {
                foundname = FindCommandName(command, cmd)!;
                if (foundname is not null)
                {
                    foundcommand = command;
                    return true;
                }
            }

        foundcommand = default;
        foundname = null;
        return false;
    }
    public IEnumerable<Command> GetMultipleByCommandString(string cmd)
    {
        var index = cmd.IndexOf(' ');
        var firstWord = index < 0 ? cmd : cmd.Substring(0, index);

        if (CommandsByFirstWord.TryGetValue(firstWord, out var cmds))
            return cmds.Where(x => CheckCommandName(x, cmd));

        return Enumerable.Empty<Command>();
    }
    static bool CheckCommandName(Command command, string cmd) => FindCommandName(command, cmd) != null;
    static string? FindCommandName(Command command, string cmd)
    {
        if (cmd is null) return null;

        return command.Names.FirstOrDefault(name =>
            cmd.Length >= name.Length
                && cmd.StartsWith(name, StringComparison.OrdinalIgnoreCase)
                && (name.Length == cmd.Length || cmd[name.Length] == ' '));
    }


    public void Add(IEnumerable<Command> commands)
    {
        foreach (var cmd in commands)
            Add(cmd);
    }
    public void Add(Command command)
    {
        CommandsList.Add(command);

        foreach (var name in command.Names)
        {
            var index = name.IndexOf(' ');
            var word = (index < 0 ? name : name.Substring(0, index)).ToLowerInvariant();
            if (!CommandsByFirstWord.ContainsKey(word))
                CommandsByFirstWord.Add(word, new List<Command>());

            CommandsByFirstWord[word].Add(command);
        }
    }

    public IEnumerator<Command> GetEnumerator() => CommandsList.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => CommandsList.GetEnumerator();
}