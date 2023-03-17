using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Zomlib;

public static class Extensions
{
    public static T ThrowIfNull<T>([NotNull] this T? value, string? message = null, [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        if (value is null)
            throw new NullReferenceException(message ?? $"{expression ?? "Value"} is null");

        return value;
    }

    public static T With<T>(this T t, Action<T> action)
    {
        action(t);
        return t;
    }

    public static void Consume(this Task task) => task.ContinueWith(t =>
    {
        if (t.Exception is not null)
        {
            LogManager.GetCurrentClassLogger().Error(t.Exception.Message);
            throw t.Exception;
        }
    }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
}
