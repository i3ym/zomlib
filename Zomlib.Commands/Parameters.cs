namespace Zomlib.Commands;

public abstract record ParametersBase
{
    public readonly ImmutableArray<ICommandParameter> Parameters;
    public Func<MessageInfo, OperationResult> PreCheck { get; init; }

    public ParametersBase(Func<MessageInfo, OperationResult> precheck, ImmutableArray<ICommandParameter> paramerers)
    {
        PreCheck = precheck;
        Parameters = paramerers;
    }

    public static OperationResult ErrorFromParameter(ICommandParameter parameter, string? error) =>
        OperationResult.Err("Ошибка в параметре '" + parameter.Name + "':" + Environment.NewLine + (error ?? Error.UnknownError.AsString()));
    public static OperationResult ErrorFromParameter(Command command, ICommandParameter parameter, string? error) =>
        OperationResult.Err(char.ToUpperInvariant(command.Names[0][0]).ToString() + command.Names[0].Substring(1) + ": ошибка в параметре '" + parameter.Name + "':" + Environment.NewLine + (error ?? Error.UnknownError.AsString()));

    public static OperationResult<T> GetParameter<T>(MessageInfo info, string[] parameters, ref int index, ICommandParameter<T> parameter)
    {
        if (parameters.Length <= index)
        {
            if (parameter.Hidden) return parameter.ConvertCheck(info, parameters, ref index);
            if (parameter.Required) return OperationResult.Err();

            return parameter.DefaultValue;
        }

        return parameter.ConvertCheck(info, parameters, ref index);
        // ErrorFromParameter(parameter, convert.AsString());
    }

    public abstract CommandResult Execute(MessageInfo info, string[] parameters);
}
public abstract record ParametersBase<TFunc> : ParametersBase where TFunc : Delegate
{
    public TFunc ExecuteFunc { get; init; }

    public ParametersBase(Func<MessageInfo, OperationResult> precheck, ImmutableArray<ICommandParameter> parameters, TFunc func) : base(precheck, parameters) => ExecuteFunc = func;
}
public record Parameters : ParametersBase<Func<string?>>
{
    public Parameters(Func<MessageInfo, OperationResult> precheck, Func<string?> func) : base(precheck, ImmutableArray<ICommandParameter>.Empty, func) { }

    public override CommandResult Execute(MessageInfo info, string[] __)
    {
        var check = PreCheck(info);
        if (!check) return new CommandResult(CommandResult.Result.Other, -1, check.AsString());

        return new CommandResult(ExecuteFunc());
    }


    public static Parameters From(Func<string?> func) =>
        From(_ => true, func);
    public static Parameters<T1> From<T1>(ICommandParameter<T1> p1, Func<T1, string?> func) =>
        From(_ => true, p1, func);
    public static Parameters<T1, T2> From<T1, T2>(ICommandParameter<T1> p1, ICommandParameter<T2> p2, Func<T1, T2, string?> func) =>
        From(_ => true, p1, p2, func);
    public static Parameters<T1, T2, T3> From<T1, T2, T3>(ICommandParameter<T1> p1, ICommandParameter<T2> p2, ICommandParameter<T3> p3, Func<T1, T2, T3, string?> func) =>
        From(_ => true, p1, p2, p3, func);
    public static Parameters<T1, T2, T3, T4> From<T1, T2, T3, T4>(ICommandParameter<T1> p1, ICommandParameter<T2> p2, ICommandParameter<T3> p3, ICommandParameter<T4> p4, Func<T1, T2, T3, T4, string?> func) =>
        From(_ => true, p1, p2, p3, p4, func);
    public static Parameters<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(ICommandParameter<T1> p1, ICommandParameter<T2> p2, ICommandParameter<T3> p3, ICommandParameter<T4> p4, ICommandParameter<T5> p5, Func<T1, T2, T3, T4, T5, string?> func) =>
        From(_ => true, p1, p2, p3, p4, p5, func);

    public static Parameters From(Func<MessageInfo, OperationResult> precheck, Func<string?> func) =>
        new(precheck, func);
    public static Parameters<T1> From<T1>(Func<MessageInfo, OperationResult> precheck, ICommandParameter<T1> p1, Func<T1, string?> func) =>
        new(precheck, p1, func);
    public static Parameters<T1, T2> From<T1, T2>(Func<MessageInfo, OperationResult> precheck, ICommandParameter<T1> p1, ICommandParameter<T2> p2, Func<T1, T2, string?> func) =>
        new(precheck, p1, p2, func);
    public static Parameters<T1, T2, T3> From<T1, T2, T3>(Func<MessageInfo, OperationResult> precheck, ICommandParameter<T1> p1, ICommandParameter<T2> p2, ICommandParameter<T3> p3, Func<T1, T2, T3, string?> func) =>
        new(precheck, p1, p2, p3, func);
    public static Parameters<T1, T2, T3, T4> From<T1, T2, T3, T4>(Func<MessageInfo, OperationResult> precheck, ICommandParameter<T1> p1, ICommandParameter<T2> p2, ICommandParameter<T3> p3, ICommandParameter<T4> p4, Func<T1, T2, T3, T4, string?> func) =>
        new(precheck, p1, p2, p3, p4, func);
    public static Parameters<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(Func<MessageInfo, OperationResult> precheck, ICommandParameter<T1> p1, ICommandParameter<T2> p2, ICommandParameter<T3> p3, ICommandParameter<T4> p4, ICommandParameter<T5> p5, Func<T1, T2, T3, T4, T5, string?> func) =>
        new(precheck, p1, p2, p3, p4, p5, func);
}