namespace Zomlib.Commands;

public record Parameters<T1> : ParametersBase<Func<T1, string?>>
{
    public readonly ICommandParameter<T1> Parameter1;

    public Parameters(Func<MessageInfo, OperationResult> precheck, ICommandParameter<T1> parameter1, Func<T1, string?> func)
        : base(precheck, new ICommandParameter[] { parameter1 }.ToImmutableArray(), func)
    {
        Parameter1 = parameter1;
    }

    public override CommandResult Execute(MessageInfo info, string[] parameters)
    {
        var check = PreCheck(info);
        if (!check) return new CommandResult(CommandResult.Result.Other, -1, check.AsString());

        int index = 0;
        if (GetParameter(info, parameters, ref index, Parameter1) is var c1 && !c1) return new CommandResult(0, c1.AsString());

        return new CommandResult(ExecuteFunc(c1.Value));
    }
}

public record Parameters<T1, T2> : ParametersBase<Func<T1, T2, string?>>
{
    public readonly ICommandParameter<T1> Parameter1;
    public readonly ICommandParameter<T2> Parameter2;

    public Parameters(Func<MessageInfo, OperationResult> precheck, ICommandParameter<T1> parameter1, ICommandParameter<T2> parameter2, Func<T1, T2, string?> func)
        : base(precheck, new ICommandParameter[] { parameter1, parameter2 }.ToImmutableArray(), func)
    {
        Parameter1 = parameter1;
        Parameter2 = parameter2;
    }

    public override CommandResult Execute(MessageInfo info, string[] parameters)
    {
        var check = PreCheck(info);
        if (!check) return new CommandResult(CommandResult.Result.Other, -1, check.AsString());

        int index = 0;
        if (GetParameter(info, parameters, ref index, Parameter1) is var c1 && !c1) return new CommandResult(0, c1.AsString());
        if (GetParameter(info, parameters, ref index, Parameter2) is var c2 && !c2) return new CommandResult(1, c2.AsString());

        return new CommandResult(ExecuteFunc(c1.Value, c2.Value));
    }
}

public record Parameters<T1, T2, T3> : ParametersBase<Func<T1, T2, T3, string?>>
{
    public readonly ICommandParameter<T1> Parameter1;
    public readonly ICommandParameter<T2> Parameter2;
    public readonly ICommandParameter<T3> Parameter3;

    public Parameters(Func<MessageInfo, OperationResult> precheck, ICommandParameter<T1> parameter1, ICommandParameter<T2> parameter2, ICommandParameter<T3> parameter3, Func<T1, T2, T3, string?> func)
        : base(precheck, new ICommandParameter[] { parameter1, parameter2, parameter3 }.ToImmutableArray(), func)
    {
        Parameter1 = parameter1;
        Parameter2 = parameter2;
        Parameter3 = parameter3;
    }

    public override CommandResult Execute(MessageInfo info, string[] parameters)
    {
        var check = PreCheck(info);
        if (!check) return new CommandResult(CommandResult.Result.Other, -1, check.AsString());

        int index = 0;
        if (GetParameter(info, parameters, ref index, Parameter1) is var c1 && !c1) return new CommandResult(0, c1.AsString());
        if (GetParameter(info, parameters, ref index, Parameter2) is var c2 && !c2) return new CommandResult(1, c2.AsString());
        if (GetParameter(info, parameters, ref index, Parameter3) is var c3 && !c3) return new CommandResult(2, c3.AsString());

        return new CommandResult(ExecuteFunc(c1.Value, c2.Value, c3.Value));
    }
}

public record Parameters<T1, T2, T3, T4> : ParametersBase<Func<T1, T2, T3, T4, string?>>
{
    public readonly ICommandParameter<T1> Parameter1;
    public readonly ICommandParameter<T2> Parameter2;
    public readonly ICommandParameter<T3> Parameter3;
    public readonly ICommandParameter<T4> Parameter4;

    public Parameters(Func<MessageInfo, OperationResult> precheck, ICommandParameter<T1> parameter1, ICommandParameter<T2> parameter2, ICommandParameter<T3> parameter3, ICommandParameter<T4> parameter4, Func<T1, T2, T3, T4, string?> func)
        : base(precheck, new ICommandParameter[] { parameter1, parameter2, parameter3, parameter4 }.ToImmutableArray(), func)
    {
        Parameter1 = parameter1;
        Parameter2 = parameter2;
        Parameter3 = parameter3;
        Parameter4 = parameter4;
    }

    public override CommandResult Execute(MessageInfo info, string[] parameters)
    {
        var check = PreCheck(info);
        if (!check) return new CommandResult(CommandResult.Result.Other, -1, check.AsString());

        int index = 0;
        if (GetParameter(info, parameters, ref index, Parameter1) is var c1 && !c1) return new CommandResult(0, c1.AsString());
        if (GetParameter(info, parameters, ref index, Parameter2) is var c2 && !c2) return new CommandResult(1, c2.AsString());
        if (GetParameter(info, parameters, ref index, Parameter3) is var c3 && !c3) return new CommandResult(2, c3.AsString());
        if (GetParameter(info, parameters, ref index, Parameter4) is var c4 && !c4) return new CommandResult(3, c4.AsString());

        return new CommandResult(ExecuteFunc(c1.Value, c2.Value, c3.Value, c4.Value));
    }
}

public record Parameters<T1, T2, T3, T4, T5> : ParametersBase<Func<T1, T2, T3, T4, T5, string?>>
{
    public readonly ICommandParameter<T1> Parameter1;
    public readonly ICommandParameter<T2> Parameter2;
    public readonly ICommandParameter<T3> Parameter3;
    public readonly ICommandParameter<T4> Parameter4;
    public readonly ICommandParameter<T5> Parameter5;

    public Parameters(Func<MessageInfo, OperationResult> precheck, ICommandParameter<T1> parameter1, ICommandParameter<T2> parameter2, ICommandParameter<T3> parameter3, ICommandParameter<T4> parameter4, ICommandParameter<T5> parameter5, Func<T1, T2, T3, T4, T5, string?> func)
        : base(precheck, new ICommandParameter[] { parameter1, parameter2, parameter3, parameter4, parameter5 }.ToImmutableArray(), func)
    {
        Parameter1 = parameter1;
        Parameter2 = parameter2;
        Parameter3 = parameter3;
        Parameter4 = parameter4;
        Parameter5 = parameter5;
    }

    public override CommandResult Execute(MessageInfo info, string[] parameters)
    {
        var check = PreCheck(info);
        if (!check) return new CommandResult(CommandResult.Result.Other, -1, check.AsString());

        int index = 0;
        if (GetParameter(info, parameters, ref index, Parameter1) is var c1 && !c1) return new CommandResult(0, c1.AsString());
        if (GetParameter(info, parameters, ref index, Parameter2) is var c2 && !c2) return new CommandResult(1, c2.AsString());
        if (GetParameter(info, parameters, ref index, Parameter3) is var c3 && !c3) return new CommandResult(2, c3.AsString());
        if (GetParameter(info, parameters, ref index, Parameter4) is var c4 && !c4) return new CommandResult(3, c4.AsString());
        if (GetParameter(info, parameters, ref index, Parameter5) is var c5 && !c5) return new CommandResult(4, c5.AsString());

        return new CommandResult(ExecuteFunc(c1.Value, c2.Value, c3.Value, c4.Value, c5.Value));
    }
}

public record Parameters<T1, T2, T3, T4, T5, T6> : ParametersBase<Func<T1, T2, T3, T4, T5, T6, string?>>
{
    public readonly ICommandParameter<T1> Parameter1;
    public readonly ICommandParameter<T2> Parameter2;
    public readonly ICommandParameter<T3> Parameter3;
    public readonly ICommandParameter<T4> Parameter4;
    public readonly ICommandParameter<T5> Parameter5;
    public readonly ICommandParameter<T6> Parameter6;

    public Parameters(Func<MessageInfo, OperationResult> precheck, ICommandParameter<T1> parameter1, ICommandParameter<T2> parameter2, ICommandParameter<T3> parameter3, ICommandParameter<T4> parameter4, ICommandParameter<T5> parameter5, ICommandParameter<T6> parameter6, Func<T1, T2, T3, T4, T5, T6, string?> func)
        : base(precheck, new ICommandParameter[] { parameter1, parameter2, parameter3, parameter4, parameter5, parameter6 }.ToImmutableArray(), func)
    {
        Parameter1 = parameter1;
        Parameter2 = parameter2;
        Parameter3 = parameter3;
        Parameter4 = parameter4;
        Parameter5 = parameter5;
        Parameter6 = parameter6;
    }

    public override CommandResult Execute(MessageInfo info, string[] parameters)
    {
        var check = PreCheck(info);
        if (!check) return new CommandResult(CommandResult.Result.Other, -1, check.AsString());

        int index = 0;
        if (GetParameter(info, parameters, ref index, Parameter1) is var c1 && !c1) return new CommandResult(0, c1.AsString());
        if (GetParameter(info, parameters, ref index, Parameter2) is var c2 && !c2) return new CommandResult(1, c2.AsString());
        if (GetParameter(info, parameters, ref index, Parameter3) is var c3 && !c3) return new CommandResult(2, c3.AsString());
        if (GetParameter(info, parameters, ref index, Parameter4) is var c4 && !c4) return new CommandResult(3, c4.AsString());
        if (GetParameter(info, parameters, ref index, Parameter5) is var c5 && !c5) return new CommandResult(4, c5.AsString());
        if (GetParameter(info, parameters, ref index, Parameter6) is var c6 && !c6) return new CommandResult(5, c6.AsString());

        return new CommandResult(ExecuteFunc(c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value));
    }
}