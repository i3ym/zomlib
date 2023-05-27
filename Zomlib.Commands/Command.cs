namespace Zomlib.Commands;

public readonly struct Command
{
    public string FullCommandString => Description + ": " + Names[0] + " " + ParametersString;
    public string ParametersString =>
        string.Join(" ",
            Parameters.Parameters
            .Where(x => !x.Hidden)
            .Select(x => x.Required ? ("<" + x.Name + ">") : ("[" + x.Name + "]"))
        );

    public readonly ImmutableArray<string> Names;
    public readonly string Description;
    public readonly ParametersBase Parameters;

    public Command(Func<MessageInfo, OperationResult> precheck, string[] names, string description, ParametersBase parameters) : this(names.ToImmutableArray(), description, parameters) =>
        parameters.GetType().GetProperty(nameof(Parameters.PreCheck))!.SetValue(parameters, precheck);

    public Command(string[] names, string description, ParametersBase parameters) : this(names.ToImmutableArray(), description, parameters) { }
    public Command(ImmutableArray<string> names, string description, ParametersBase parameters)
    {
        Names = names;
        Description = description;
        Parameters = parameters;
    }
}