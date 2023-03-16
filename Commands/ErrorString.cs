using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Zomlib.Commandss;

interface IErrorString
{
    bool Success { get; }
    string? Message { get; }
}

[AsyncMethodBuilder(typeof(ErrorStringTaskMethodBuilder))]
public readonly struct ErrorString : IErrorString
{
    bool IErrorString.Success => Success;
    string? IErrorString.Message => Message;

    public readonly bool Success;
    public readonly string? Message;

    public ErrorString(bool success) : this(success, null) { }
    public ErrorString(string? message) : this(true, message) { }
    public ErrorString(bool success, string? message)
    {
        Success = success;
        Message = message;
    }

    public string AsString() => Message ?? string.Empty;
    public override string? ToString() => AsString();

    public ErrorString<T> As<T>(T? value = default) => new ErrorString<T>(this, value);


    public static ErrorString Err(string? msg = null) => new ErrorString(false, msg);
    public static ErrorString<T> Err<T>(string? msg = null) => Err(msg);
    public static ErrorString<T> Succ<T>(T value) => new ErrorString<T>(value);

    public static implicit operator bool(ErrorString es) => es.Success;
    public static implicit operator ErrorString(bool success) => new ErrorString(success);
    public static implicit operator ErrorString(string? output) => new ErrorString(true, output);


    public Awaiter GetAwaiter() => new(this);

    public struct Awaiter : INotifyCompletion, IErrorString
    {
        bool IErrorString.Success => Value.Success;
        string? IErrorString.Message => Value.Message;

        public bool IsCompleted { get; private set; }
        public ErrorString Value { get; }

        public Awaiter(ErrorString value)
        {
            IsCompleted = false;
            Value = value;
        }


        public void OnCompleted(Action continuation)
        {
            IsCompleted = true;

            if (!Value.Success) return;
            continuation();
        }

        public void GetResult() { }
    }
}

[AsyncMethodBuilder(typeof(ErrorStringTaskMethodBuilder<>))]
public readonly struct ErrorString<T> : IErrorString
{
    public bool Success => EString.Success;
    public string? Message => EString.Message;

    public readonly ErrorString EString;
    [AllowNull] public readonly T Value;

    public ErrorString(bool success, string? message, T? value = default) : this(new ErrorString(success, message), value) { }
    public ErrorString(T? value) : this(true, value) { }
    public ErrorString(ErrorString es, T? value = default)
    {
        EString = es;
        Value = value;
    }

    public ErrorString AsEString() => EString;

    public bool TryGet([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out string error)
    {
        value = Value;
        error = Message;
        return Success;
    }

    public string AsString() => EString.AsString();
    public override string? ToString() => EString.ToString();

    public ErrorString<T2> As<T2>(Func<T, T2> convert) => new ErrorString<T2>(EString, convert(Value));

    public static implicit operator bool(ErrorString<T> es) => es.Success;
    public static implicit operator ErrorString<T>(bool success) => new ErrorString<T>(success);
    public static implicit operator ErrorString<object>(ErrorString<T> es) => new ErrorString<object>(es.Value);
    public static implicit operator ErrorString<T>(ErrorString es) => new ErrorString<T>(es);
    public static implicit operator ErrorString<T>(T t) => new ErrorString<T>(true, t);


    public Awaiter GetAwaiter() => new(this);

    public struct Awaiter : INotifyCompletion, IErrorString
    {
        bool IErrorString.Success => Value.Success;
        string? IErrorString.Message => Value.Message;

        public bool IsCompleted { get; private set; }
        public ErrorString<T> Value { get; }

        public Awaiter(ErrorString<T> value)
        {
            IsCompleted = false;
            Value = value;
        }


        public void OnCompleted(Action continuation)
        {
            IsCompleted = true;

            if (!Value.Success) return;
            continuation();
        }

        public T GetResult() => Value.Value;
    }
}



public class ErrorStringTaskMethodBuilder<T>
{
    public ErrorString<T> Task { get; private set; }

    public ErrorStringTaskMethodBuilder() => Task = new();
    public static ErrorStringTaskMethodBuilder<T> Create() => new();

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();
    public void SetStateMachine(IAsyncStateMachine _) { }

    public void SetException(Exception _) => System.Diagnostics.Debugger.Break(); // Task.SetException(exception);
    public void SetResult(T result) => Task = new(true, result);

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine =>
        GenericAwaitOnCompleted(ref awaiter, ref stateMachine);
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine =>
        GenericAwaitOnCompleted(ref awaiter, ref stateMachine);
    public void GenericAwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        if (awaiter is ErrorString<T>.Awaiter atw)
            Task = atw.Value;
        else if (awaiter is IErrorString twa && !twa.Success)
            Task = ErrorString.Err(twa.Message);

        awaiter.OnCompleted(stateMachine.MoveNext);
    }
}
public class ErrorStringTaskMethodBuilder
{
    public ErrorString Task { get; private set; }

    public ErrorStringTaskMethodBuilder() => Task = new();
    public static ErrorStringTaskMethodBuilder Create() => new();

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();
    public void SetStateMachine(IAsyncStateMachine _) { }

    public void SetException(Exception _) => System.Diagnostics.Debugger.Break(); // Task.SetException(exception);
    public void SetResult() => Task = new(true);

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine =>
        GenericAwaitOnCompleted(ref awaiter, ref stateMachine);
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine =>
        GenericAwaitOnCompleted(ref awaiter, ref stateMachine);
    public void GenericAwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        if (awaiter is ErrorString.Awaiter atw)
            Task = atw.Value;
        else if (awaiter is IErrorString twa && !twa.Success)
            Task = ErrorString.Err(twa.Message);

        awaiter.OnCompleted(stateMachine.MoveNext);
    }
}