namespace StayFit.Domain.Results;

/// <summary>
/// Базовий Result патерн для обробки операцій без throw
/// </summary>
public abstract class Result
{
    public sealed class Success : Result;
    public sealed class SuccessWithData<T> : Result
    {
        public T Data { get; set; }
        public SuccessWithData(T data) => Data = data;
    }
    public sealed class Failure : Result
    {
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public Failure(string errorMessage, string errorCode = "UNKNOWN_ERROR")
        {
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
    }

    public TResult Match<TResult>(
        Func<Success, TResult> onSuccess,
        Func<SuccessWithData<IEnumerable<object>>, TResult> onSuccessWithData,
        Func<Failure, TResult> onFailure)
    {
        return this switch
        {
            Success success => onSuccess(success),
            SuccessWithData<IEnumerable<object>> data => onSuccessWithData(data),
            Failure failure => onFailure(failure),
            _ => throw new InvalidOperationException("Unknown result type")
        };
    }

    public void Match(
        Action<Success> onSuccess,
        Action<SuccessWithData<IEnumerable<object>>> onSuccessWithData,
        Action<Failure> onFailure)
    {
        switch (this)
        {
            case Success success:
                onSuccess(success);
                break;
            case SuccessWithData<IEnumerable<object>> data:
                onSuccessWithData(data);
                break;
            case Failure failure:
                onFailure(failure);
                break;
        }
    }

    public bool IsSuccess => this is Success or SuccessWithData<IEnumerable<object>>;
    public bool IsFailure => this is Failure;
}

/// <summary>
/// Generic Result з типом даних
/// </summary>
public abstract class Result<T>
{
    public sealed class Success : Result<T>
    {
        public T Data { get; set; }
        public Success(T data) => Data = data;
    }
    public sealed class Failure : Result<T>
    {
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public Failure(string errorMessage, string errorCode = "UNKNOWN_ERROR")
        {
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
    }

    public TResult Match<TResult>(
        Func<Success, TResult> onSuccess,
        Func<Failure, TResult> onFailure)
    {
        return this switch
        {
            Success success => onSuccess(success),
            Failure failure => onFailure(failure),
            _ => throw new InvalidOperationException("Unknown result type")
        };
    }

    public void Match(
        Action<Success> onSuccess,
        Action<Failure> onFailure)
    {
        switch (this)
        {
            case Success success:
                onSuccess(success);
                break;
            case Failure failure:
                onFailure(failure);
                break;
        }
    }

    public bool IsSuccess => this is Success;
    public bool IsFailure => this is Failure;
}
