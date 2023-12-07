namespace Sechat.Service.Configuration.Mediator.Responses;

public class MediatorResult<TResult>
{
    public TResult Result { get; }
    public bool Success { get; }
    public string ErrorMessage { get; }

    public MediatorResult(TResult result, bool success)
    {
        Result = result;
        Success = success;
    }

    public MediatorResult(TResult result, bool success, string errorMessage) : this(result, success) => ErrorMessage = errorMessage;

}
