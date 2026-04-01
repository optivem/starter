namespace Common;

public class Result<T, E>
{
    private readonly bool _success;
    private readonly T? _value;
    private readonly E? _error;

    private Result(bool success, T? value, E? error)
    {
        _success = success;
        _value = value;
        _error = error;
    }

    public static Result<T, E> Success(T value) => new(true, value, default);

    public static Result<T, E> Failure(E error) => new(false, default, error);

    public bool IsSuccess => _success;

    public bool IsFailure => !_success;

    public T Value
    {
        get
        {
            if (!_success)
                throw new InvalidOperationException("Cannot get value from a failed result");
            return _value!;
        }
    }

    public E Error
    {
        get
        {
            if (_success)
                throw new InvalidOperationException("Cannot get error from a successful result");
            return _error!;
        }
    }

    public Result<T2, E> Map<T2>(Func<T, T2> mapper)
    {
        if (_success)
        {
            return Result<T2, E>.Success(mapper(_value!));
        }
        return Result<T2, E>.Failure(_error!);
    }

    public Result<T, E2> MapError<E2>(Func<E, E2> mapper)
    {
        if (_success)
        {
            return Result<T, E2>.Success(_value!);
        }
        return Result<T, E2>.Failure(mapper(_error!));
    }

    public Result<VoidValue, E> MapVoid()
    {
        if (_success)
        {
            return Result<VoidValue, E>.Success(VoidValue.Empty);
        }
        return Result<VoidValue, E>.Failure(_error!);
    }
}

public static class Result
{
    public static Result<VoidValue, E> Success<E>()
        => Result<VoidValue, E>.Success(VoidValue.Empty);

    public static Result<VoidValue, E> Failure<E>(E error)
        => Result<VoidValue, E>.Failure(error);
}
