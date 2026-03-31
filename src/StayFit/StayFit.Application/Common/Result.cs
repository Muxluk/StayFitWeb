namespace StayFit.Application.Common;

public class Result
{
    protected Result(bool isSuccess, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyList<string> Errors { get; }

    public static Result Success() => new(true, Array.Empty<string>());

    public static Result Failure(params string[] errors) =>
        new(false, errors.Length == 0 ? ["Operation failed."] : errors);

    public static Result Failure(IReadOnlyList<string> errors) =>
        new(false, errors.Count == 0 ? ["Operation failed."] : errors);
}

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, IReadOnlyList<string> errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) =>
        new(true, value, Array.Empty<string>());

    public static implicit operator Result<T>(T value) => Success(value);

    public new static Result<T> Failure(params string[] errors) =>
        new(false, default, errors.Length == 0 ? ["Operation failed."] : errors);

    public new static Result<T> Failure(IReadOnlyList<string> errors) =>
        new(false, default, errors.Count == 0 ? ["Operation failed."] : errors);
}
