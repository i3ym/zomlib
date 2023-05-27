namespace Zomlib.Commands;

public static class Error
{
    public static readonly OperationResult
        Success = true,
        InvalidArgument = Err("Неверный аргумент"),
        NotEnoughArguments = Err("Недостаточно верных аргументов"),
        NotEnoughPermission = Err("Недостаточно прав"),
        ArgumentMustBeNumber = Err("Не число"),
        ArgumentMustBePositive = Err("Число должно быть больше нуля"),
        ArgumentIsRequired = Err("Параметр обязателен"),

        IncorrectChoice = Err("Неверный выбор"),
        UnknownError = Err("Неизвестная ошибка");


    public static OperationResult Err(this string error) => OperationResult.Err(error);
}