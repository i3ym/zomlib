namespace Zomlib.Commands;

public readonly struct CommandResult
{
    public readonly Result Status;
    public readonly int ErrorParameterIndex;
    public readonly string? Message;

    public CommandResult(Result status, int errorParameterIndex, string? result)
    {
        Status = status;
        ErrorParameterIndex = errorParameterIndex;
        Message = result;
    }
    public CommandResult(int errorParameterIndex, string? error) : this(string.IsNullOrEmpty(error) ? Result.NotEnoughParameters : Result.ParameterError, errorParameterIndex, error) { }
    public CommandResult(string? result) : this(Result.Success, -1, result) { }

    public static implicit operator bool(CommandResult result) => result.Status == Result.Success;


    public enum Result
    {
        Success,
        ParameterError,
        NotEnoughParameters,
        Other,
    }
}