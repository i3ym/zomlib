using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Zomlib;

public static class Extensions
{
    public static async ValueTask<T> ThrowIfNull<T>([NotNull] this ValueTask<T?> task, string? message = null, [CallerArgumentExpression(nameof(task))] string? expression = null) =>
        (await task).ThrowIfNull(message, expression);
    public static async Task<T> ThrowIfNull<T>([NotNull] this Task<T?> task, string? message = null, [CallerArgumentExpression(nameof(task))] string? expression = null) =>
        (await task).ThrowIfNull(message, expression);
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

    public static void Consume(this Task task)
    {
        task.ContinueWith(t =>
        {
            if (t.Exception is not null)
                throw t.Exception;
        }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
    }
}
