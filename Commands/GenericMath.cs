using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Zomlib.Commands;

public static class GenericMath
{
    public static Func<ValueType, string> MoneyFormatter = x => x.ToString()!;

    public static OperationResult<T> TryParse<T>(string text, T? min = default, T? max = default) where T : unmanaged, INumber<T> => _TryParse<T>(text, min ?? GenericMath.MinValue<T>(), max ?? GenericMath.MaxValue<T>());
    static OperationResult<T> _TryParse<T>(string text, T customMin, T customMax) where T : unmanaged, INumber<T>
    {
        text = text.Trim().ToLowerInvariant();
        if (text.Length == 0) return Error.ArgumentMustBeNumber;

        T? known = text switch
        {
            "#r" => Random<T>(customMin, customMax),
            "мин" or "min" => customMin,
            "все" or "всё" or "макс" or "max" => customMax,
            "половина" => customMax / T.CreateSaturating(2),
            "треть" => customMax / T.CreateSaturating(3),
            "четверть" => customMax / T.CreateSaturating(4),
            "сто" or "сотня" => T.CreateSaturating(100),
            "тысяча" or "тыща" => T.CreateSaturating(1_000),
            "миллион" or "милион" => T.CreateSaturating(1_000_000),
            "миллиард" or "милиард" => T.CreateSaturating(1_000_000_000),
            _ => null,
        };
        if (known is not null) return known.Value;

        if (text[0] != '-' && text.Contains('-') && text.Count(x => x == '-') == 1 && text.Last() != '-')
        {
            var spt = text.Split('-');

            var min = _TryParse<T>(spt[0], customMin, customMax);
            if (!min) return min;
            if (min.Value < customMin) return OperationResult.Err("Первое число должно быть не меньше " + MoneyFormatter(customMin));

            var max = _TryParse<T>(spt[1], customMin, customMax);
            if (!max) return max;
            if (max.Value > customMax) return OperationResult.Err("Второе число должно быть не больше " + MoneyFormatter(customMax));

            if (min.Value > max.Value) return OperationResult.Err("Первое число должно быть меньше второго");
            return Random(min.Value, max.Value);
        }

        if (!GenericMath.TryParse<T>(text.Replace("_", "").Replace("k", "").Replace("m", "").Replace("к", "").Replace("м", ""), out var value))
            return Error.ArgumentMustBeNumber;

        var textspan = text.AsSpan();
        while (true)
        {
            if (textspan[^1] == 'к' || textspan[^1] == 'k') value *= T.CreateSaturating(1_000);
            else if (textspan[^1] == 'м' || textspan[^1] == 'm') value *= T.CreateSaturating(1_000_000);
            else break;

            textspan = textspan[0..^1];
        }

        if (value < customMin) return OperationResult.Err("Число должно быть не меньше " + MoneyFormatter(customMin));
        if (value > customMax) return OperationResult.Err("Число должно быть не больше " + MoneyFormatter(customMax));

        return value;
    }
    public static T Random<T>(T min, T max) where T : unmanaged, INumber<T>
    {
        if (min > max) throw new ArgumentOutOfRangeException();

        Span<T> value = stackalloc T[1];
        System.Random.Shared.NextBytes(MemoryMarshal.Cast<T, byte>(value));

        var output = value[0];
        return T.Abs(output % (max - min)) + min;
    }



    public static bool TryParse<T>(string parameter, [MaybeNullWhen(false)] out T value) where T : unmanaged, INumber<T> => T.TryParse(parameter, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    public static T Parse<T>(string parameter) where T : unmanaged, INumber<T> => T.Parse(parameter, CultureInfo.InvariantCulture);

    public static T MinValue<T>() where T : unmanaged, INumber<T> => (T) typeof(T).GetField(nameof(int.MinValue))!.GetValue(null)!;
    public static T MaxValue<T>() where T : unmanaged, INumber<T> => (T) typeof(T).GetField(nameof(int.MaxValue))!.GetValue(null)!;
}