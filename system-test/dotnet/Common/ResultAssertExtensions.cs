using Shouldly;

namespace Common;

public static class ResultAssertExtensions
{
    public static Result<T, E> ShouldBeSuccess<T, E>(this Result<T, E> result)
    {
        if (!result.IsSuccess)
        {
            var error = result.Error;
            throw new ShouldAssertException($"Expected result to be success but was failure with error: {error}");
        }
        return result;
    }

    public static Result<T, E> ShouldBeFailure<T, E>(this Result<T, E> result)
    {
        result.IsFailure.ShouldBeTrue("Expected result to be failure but was success");
        return result;
    }
}
