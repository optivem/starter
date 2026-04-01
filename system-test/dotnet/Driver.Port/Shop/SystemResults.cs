using Common;
using Driver.Port.Shop.Dtos.Error;

namespace Driver.Port.Shop;

public static class SystemResults
{
    public static Result<T, SystemError> Success<T>(T value)
    {
        return Result<T, SystemError>.Success(value);
    }

    public static Result<VoidValue, SystemError> Success()
    {
        return Result.Success<SystemError>();
    }

    public static Result<VoidValue, SystemError> Failure(string message)
    {
        return Result<VoidValue, SystemError>.Failure(SystemError.Of(message));
    }

    public static Result<VoidValue, SystemError> Failure(SystemError error)
    {
        return Result<VoidValue, SystemError>.Failure(error);
    }

    public static Result<T, SystemError> Failure<T>(string message)
    {
        return Result<T, SystemError>.Failure(SystemError.Of(message));
    }

    public static Result<T, SystemError> Failure<T>(SystemError error)
    {
        return Result<T, SystemError>.Failure(error);
    }
}

