using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Zomlib;

public readonly struct OperationResult
{
    public readonly bool Success;
    public readonly string? Message;

    public OperationResult(bool success, string? message)
    {
        Success = success;
        Message = message;
    }

    [MethodImpl(256)] public string AsString() => Message ?? string.Empty;
    [MethodImpl(256)] public override string? ToString() => $"{{ Success: {Success}{(Success ? null : $", Message: {Message}")} }}";

    public OperationResult<T> As<T>(T? value = default) => new OperationResult<T>(this, value);


    public static OperationResult Err(string? msg = null) => new OperationResult(false, msg);
    public static OperationResult Err<T>(T obj) where T : notnull => Err(obj.ToString());
    public static OperationResult Err(Exception ex) => new OperationResult(false, ex.Message);
    public static OperationResult<T> Err<T>(string? msg = null) => Err(msg);
    public static OperationResult Succ() => new OperationResult(true, null);
    public static OperationResult<T> Succ<T>(T value) => new OperationResult<T>(value);

    public static OperationResult WrapException(Action func, string? info = null) => WrapException(() => { func(); return true; }, info);
    public static ValueTask<OperationResult> WrapException(Func<ValueTask> func, string? info = null) => WrapException(async () => { await func().ConfigureAwait(false); return true; }, info);
    public static OperationResult WrapException(Func<OperationResult> func, string? info = null)
    {
        try { return func(); }
        catch (Exception ex) { return MakeException(ex, info); }
    }
    public static OperationResult<T> WrapException<T>(Func<OperationResult<T>> func, string? info = null)
    {
        try { return func(); }
        catch (Exception ex) { return MakeException(ex, info); }
    }
    public static async ValueTask<OperationResult> WrapException(Func<ValueTask<OperationResult>> func, string? info = null)
    {
        try { return await func(); }
        catch (Exception ex) { return MakeException(ex, info); }
    }
    public static async ValueTask<OperationResult<T>> WrapException<T>(Func<ValueTask<OperationResult<T>>> func, string? info = null)
    {
        try { return await func(); }
        catch (Exception ex) { return MakeException(ex, info); }
    }
    static OperationResult MakeException(Exception ex, string? info)
    {
        if (info is null) return OperationResult.Err(ex);
        return new OperationResult(false, @$"Got an {ex.GetType().Name} {info}: {ex.Message}");
    }

    public static implicit operator bool(OperationResult es) => es.Success;
    public static implicit operator OperationResult(bool success) => new OperationResult(success, null);
}
public readonly struct OperationResult<T>
{
    public bool Success => BaseResult.Success;
    public string? Message => BaseResult.Message;

    public readonly OperationResult BaseResult;
    [AllowNull] public readonly T Value;

    public OperationResult(bool success, string? message, T? value = default) : this(new OperationResult(success, message), value) { }
    public OperationResult(T? value) : this(true, value) { }
    public OperationResult(OperationResult es, T? value = default)
    {
        BaseResult = es;
        Value = value;
    }

    public bool TryGet([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out string error)
    {
        value = Value;
        error = Message;
        return Success;
    }

    [MethodImpl(256)] public string AsString() => BaseResult.AsString();
    [MethodImpl(256)] public override string? ToString() => $"{{ Success: {Success}, {(Success ? $"Value: {Value}" : $"Message: {Message}")} }}";

    public OperationResult<T2> As<T2>()
    {
        if (Success) throw new InvalidOperationException("Can not convert successful OperationResult");
        return new OperationResult<T2>(this.BaseResult);
    }
    public OperationResult<T2> As<T2>(Func<T, T2> convert) => new OperationResult<T2>(BaseResult, convert(Value));

    public static implicit operator bool(in OperationResult<T> es) => es.Success;
    public static implicit operator OperationResult<object>(in OperationResult<T> es) => new OperationResult<object>(es.Value);
    public static implicit operator OperationResult<T>(in OperationResult es) => new OperationResult<T>(es);
    public static implicit operator OperationResult<T>(T t) => new OperationResult<T>(true, t);
}

public static class OperationResultExtensions
{
    public static OperationResult<T> AsOpResult<T>(this T value) => OperationResult.Succ(value);
    public static ValueTask<T> AsVTask<T>(this T value) => ValueTask.FromResult(value);
    public static Task<T> AsTask<T>(this T value) => Task.FromResult(value);
    public static ValueTask<OperationResult<T>> AsTaskResult<T>(this T value) => value.AsOpResult().AsVTask();

    public static void ThrowIfError(in this OperationResult opr, string? format = null)
    {
        if (!opr)
        {
            if (format is not null) throw new Exception(string.Format(format, opr.AsString()));
            else throw new Exception(opr.AsString());
        }
    }
    public static T ThrowIfError<T>(in this OperationResult<T> opr, string? format = null)
    {
        opr.BaseResult.ThrowIfError(format);
        return opr.Value;
    }
    public static async Task ThrowIfError(this Task<OperationResult> opr, string? format = null) => (await opr).ThrowIfError(format);
    public static async Task<T> ThrowIfError<T>(this Task<OperationResult<T>> opr, string? format = null) => (await opr).ThrowIfError(format);

    public static OperationResult LogIfError(in this OperationResult opr, ILogger logger, string? format = null)
    {
        if (!opr)
        {
            if (format is not null) logger.LogError(string.Format(format, opr.AsString()));
            else logger.LogError(opr.AsString());
        }

        return opr;
    }
    public static OperationResult<T> LogIfError<T>(in this OperationResult<T> opr, ILogger logger, string? format = null)
    {
        opr.BaseResult.LogIfError(logger, format);
        return opr;
    }
    public static async Task LogIfError(this Task<OperationResult> opr, ILogger logger, string? format = null) => (await opr).LogIfError(logger, format);
    public static async Task<OperationResult<T>> LogIfError<T>(this Task<OperationResult<T>> opr, ILogger logger, string? format = null) => (await opr).LogIfError(logger, format);
}