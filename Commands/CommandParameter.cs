namespace Zomlib.Commands;

public interface ICommandParameter
{
    bool Required { get; }
    string Name { get; }
    bool Hidden { get; }
}
public interface ICommandParameter<T> : ICommandParameter
{
    T DefaultValue { get; set; }
    OperationResult Check(MessageInfo info, T value);
    OperationResult<T> Conversion(MessageInfo info, string[] parameters, ref int index);

    OperationResult<T> ConvertCheck(MessageInfo info, string[] parameters, ref int index)
    {
        var c = Conversion(info, parameters, ref index);
        if (!c) return c;

        var ch = Check(info, c.Value);
        if (!ch) return ch;

        return c;
    }
}
public interface IConvertibleParameter<TFrom, TTo> : ICommandParameter<TTo>
{
    OperationResult<TTo> Conversion(MessageInfo info, TFrom value);
}
public interface IMinMaxCommandParameter : ICommandParameter
{
    int Min { get; set; }
    int Max { get; set; }
}

public abstract class CommandParameter : ICommandParameter
{
    public string Name { get; }
    public bool Required { get; }
    public virtual bool Hidden => false;

    public CommandParameter(string name, bool required)
    {
        Name = name;
        Required = required;
    }
}
public abstract class CommandParameter<T> : CommandParameter, ICommandParameter<T>
{
    public T DefaultValue { get; set; } = default!;

    public CommandParameter(string cmdname, bool required) : base(cmdname, required) { }

    public abstract OperationResult Check(MessageInfo info, T value);
    public abstract OperationResult<T> Conversion(MessageInfo info, string[] parameters, ref int index);
    public OperationResult<T> ConvertCheck(MessageInfo info, string[] parameters, ref int index)
    {
        var c = Conversion(info, parameters, ref index);
        if (!c) return c;

        var ch = Check(info, c.Value);
        if (!ch) return ch;

        return c;
    }
}
public abstract class CmdCommandParameter<T> : CommandParameter<T>, IConvertibleParameter<string, T>
{
    public CmdCommandParameter(string cmdname, bool required) : base(cmdname, required) { }

    public sealed override OperationResult<T> Conversion(MessageInfo info, string[] parameters, ref int index) =>
        Conversion(info, parameters[index++]);
    public abstract OperationResult<T> Conversion(MessageInfo info, string parameter);
    public OperationResult<T> ConvertCheck(MessageInfo info, string parameter)
    {
        var c = Conversion(info, parameter);
        if (!c) return c;

        var ch = Check(info, c.Value);
        if (!ch) return ch;

        return c;
    }
}


public class MultiCommandParameter<T> : CommandParameter<T[]>
{
    protected readonly CommandParameter<T> CommandParameter;

    public MultiCommandParameter(CommandParameter<T> cparameter) : base(cparameter.Name, cparameter.Required) =>
        CommandParameter = cparameter;

    public override OperationResult Check(MessageInfo info, T[] values)
    {
        if (values is null || values.Length == 0) return Error.NotEnoughArguments;

        OperationResult check;
        foreach (var value in values)
        {
            check = CommandParameter.Check(info, value);
            if (!check) return check;
        }

        return true;
    }
    public override OperationResult<T[]> Conversion(MessageInfo info, string[] parameters, ref int index)
    {
        var split = parameters[index++].Split(',');
        var output = new List<T>(split.Length);

        for (int i = 0; i < split.Length;)
        {
            var conversion = CommandParameter.Conversion(info, split, ref i);
            if (!conversion) conversion.As<T[]>();

            output.Add(conversion.Value);
        }

        return new OperationResult<T[]>(output.ToArray());
    }
}
public class NullableCommandParameter<T> : CommandParameter<T?> where T : struct
{
    protected readonly CommandParameter<T> CommandParameter;

    public NullableCommandParameter(CommandParameter<T> parameter) : base(parameter.Name, parameter.Required) =>
        CommandParameter = parameter;

    public override OperationResult Check(MessageInfo info, T? value) =>
        value is null
        ? (OperationResult) true
        : CommandParameter.Check(info, value.Value);
    public override OperationResult<T?> Conversion(MessageInfo info, string[] parameters, ref int index)
    {
        var conversion = CommandParameter.Conversion(info, parameters, ref index);
        return new OperationResult<T?>(conversion.EString, new T?(conversion.Value));
    }
}
public class EitherCommandParameter<T1, T2> : CommandParameter<Either<T1, T2>>
{
    readonly ICommandParameter<T1> Parameter1;
    readonly ICommandParameter<T2> Parameter2;

    public EitherCommandParameter(ICommandParameter<T1> p1, ICommandParameter<T2> p2, string cmdname, bool required) : base(cmdname, required) =>
        (Parameter1, Parameter2) = (p1, p2);

    public override OperationResult Check(MessageInfo info, Either<T1, T2> value) =>
        value.IsLeft
            ? Parameter1.Check(info, value.Left)
            : Parameter2.Check(info, value.Right);
    public override OperationResult<Either<T1, T2>> Conversion(MessageInfo info, string[] parameters, ref int index)
    {
        var idx = index;

        var c1 = Parameter1.Conversion(info, parameters, ref index);
        if (c1) return (Either<T1, T2>) c1.Value;

        index = idx;

        var c2 = Parameter1.Conversion(info, parameters, ref index);
        if (c2) return (Either<T1, T2>) c2.Value;

        return OperationResult.Err(c1.AsString() + " | " + c2.AsString());
    }
}
public readonly struct Either<TL, TR>
{
    public readonly TL Left;
    public readonly TR Right;
    public readonly bool IsLeft;

    public Either(TL left)
    {
        Left = left;
        Right = default!;
        IsLeft = true;
    }
    public Either(TR right)
    {
        Right = right;
        Left = default!;
        IsLeft = false;
    }

    public T Match<T>(Func<TL, T> leftFunc, Func<TR, T> rightFunc) => IsLeft ? leftFunc(Left) : rightFunc(Right);

    public static implicit operator Either<TL, TR>(TL left) => new Either<TL, TR>(left);
    public static implicit operator Either<TL, TR>(TR right) => new Either<TL, TR>(right);
}

public class StringCommandParameter : CommandParameter<string>, IConvertibleParameter<string, string>, IMinMaxCommandParameter
{
    public int Min { get; set; } = 1;
    public int Max { get; set; } = int.MaxValue;

    public StringCommandParameter(string cmdname, bool required) : base(cmdname, required) { }

    public override OperationResult Check(MessageInfo info, string value)
    {
        if (!Required && string.IsNullOrEmpty(value)) return true;
        return CheckLength(Min, Max, value);
    }
    public override OperationResult<string> Conversion(MessageInfo info, string[] parameters, ref int index)
    {
        if (Required && index + 1 > parameters.Length) return Error.InvalidArgument;

        var output = string.Join(" ", parameters.Skip(index));
        index = parameters.Length;
        return output;
    }

    public OperationResult<string> Conversion(MessageInfo info, string value) => value;

    public static OperationResult CheckLength(int min, int max, string value)
    {
        if (value.Length > max) return OperationResult.Err("Текст должен быть не длиннее " + max + " симв.");
        if (value.Length < min) return OperationResult.Err("Текст должен быть не короче " + min + " симв.");

        return true;
    }
}
public class WordArrayCommandParameter : CommandParameter<string[]>
{
    public WordArrayCommandParameter(string cmdname, bool required) : base(cmdname, required) { }

    public override OperationResult Check(MessageInfo info, string[] value) => true;
    public override OperationResult<string[]> Conversion(MessageInfo info, string[] parameters, ref int index)
    {
        var output = parameters.Skip(index).ToArray();
        index = parameters.Length;
        return output;
    }
}
public class WordCommandParameter : CmdCommandParameter<string>, IMinMaxCommandParameter
{
    public int Min { get; set; } = 1;
    public int Max { get; set; } = int.MaxValue;

    public WordCommandParameter(string cmdname, bool required) : base(cmdname, required) { }


    public override OperationResult Check(MessageInfo info, string value) => StringCommandParameter.CheckLength(Min, Max, value);
    public override OperationResult<string> Conversion(MessageInfo info, string parameter) => parameter;
}
public class BoolCommandParameter : CmdCommandParameter<bool>
{
    readonly Random Random = new Random();

    public BoolCommandParameter(string cmdname, bool required) : base(cmdname, required) { }

    public override OperationResult Check(MessageInfo info, bool value) => true;
    public override OperationResult<bool> Conversion(MessageInfo info, string parameter)
    {
        parameter = parameter.ToLowerInvariant();

        return parameter switch
        {
            "1" or "да" or "y" or "д" or "+" or "вкл" or "true" => new OperationResult<bool>(true),
            "0" or "нет" or "n" or "н" or "-" or "выкл" or "false" => new OperationResult<bool>(false),
            "#r" => new OperationResult<bool>(Random.Next(2) == 0),
            _ => Error.IncorrectChoice,
        };
    }
}
public class ConfirmationCommandParameter : CmdCommandParameter<bool>
{
    protected readonly string[] Confirmations;

    public ConfirmationCommandParameter(string[] confirmations, bool required = false) : base("подтверждение", required) =>
        Confirmations = confirmations.Select(x => x.ToLowerInvariant()).ToArray();

    public override OperationResult Check(MessageInfo info, bool value) => true;
    public override OperationResult<bool> Conversion(MessageInfo info, string parameter) =>
        new OperationResult<bool>(Error.Success, null, Confirmations.Contains(parameter.ToLowerInvariant()));
}

public class NumberCommandParameter<T> : CmdCommandParameter<T> where T : unmanaged, INumber<T>
{
    public string NumberTooLowText = "Число должно быть не меньше {0}";
    public string NumberTooHighText = "Число должно быть не больше {0}";
    public T Min, Max;

    public NumberCommandParameter(string cmdname, bool required) : base(cmdname, required)
    {
        Min = GenericMath.MinValue<T>();
        Max = GenericMath.MaxValue<T>();
    }

    public override OperationResult Check(MessageInfo info, T value)
    {
        if (value < Min) return NumberIsTooLow(Min);
        if (value > Max) return NumberIsTooHigh(Max);
        return true;
    }
    public override OperationResult<T> Conversion(MessageInfo info, string parameter) => GenericMath.TryParse<T>(parameter, Min, Max);

    protected OperationResult NumberIsTooLow(T value) => OperationResult.Err(string.Format(NumberTooLowText, value));
    protected OperationResult NumberIsTooHigh(T value) => OperationResult.Err(string.Format(NumberTooHighText, value));
}
public class IntCommandParameter : NumberCommandParameter<int> { public IntCommandParameter(string cmdname, bool required) : base(cmdname, required) { } }
public class LongCommandParameter : NumberCommandParameter<long> { public LongCommandParameter(string cmdname, bool required) : base(cmdname, required) { } }
public class ULongCommandParameter : NumberCommandParameter<ulong> { public ULongCommandParameter(string cmdname, bool required) : base(cmdname, required) { } }

public class TimeSpanCommandParameter : CmdCommandParameter<TimeSpan>
{
    public TimeSpanCommandParameter(string cmdname, bool required) : base(cmdname, required) { }
    public TimeSpanCommandParameter() : this("продолжительность", true) { }

    public override OperationResult Check(MessageInfo info, TimeSpan value) => true;
    public override OperationResult<TimeSpan> Conversion(MessageInfo info, string parameter)
    {
        var multiplier = parameter[^1] switch
        {
            's' or 'с' => 1,
            'm' or 'м' => 60,
            'h' or 'ч' => 60 * 60,
            'd' or 'д' => 60 * 60 * 24,
            _ => 1,
        };
        if (!int.TryParse(parameter[..^1], out var sec))
            return Error.ArgumentMustBeNumber;

        return new TimeSpan(0, 0, sec * multiplier);
    }
}
public class EnumCommandParameter<TEnum> : CmdCommandParameter<TEnum> where TEnum : struct, Enum
{
    public EnumCommandParameter(string cmdname, bool required) : base(cmdname, required) { }

    public override OperationResult Check(MessageInfo info, TEnum value) => true;
    public override OperationResult<TEnum> Conversion(MessageInfo info, string parameter)
    {
        if (!Enum.TryParse<TEnum>(parameter, true, out var result)) return Error.InvalidArgument;
        return result;
    }
}

public class MessageInfoCommandParameter : CommandParameter<MessageInfo>
{
    public static readonly MessageInfoCommandParameter Instance = new();

    public override bool Hidden => true;

    private MessageInfoCommandParameter() : base("\\msginfo\\", false) { }

    public override OperationResult Check(MessageInfo _, MessageInfo __) => true;
    public override OperationResult<MessageInfo> Conversion(MessageInfo info, string[] _, ref int __) => info;
}