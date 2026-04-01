namespace Common;

public static class ResultTaskExtensions
{
    public static async Task<Result<T, E2>> MapErrorAsync<T, E, E2>(
        this Task<Result<T, E>> resultTask,
        Func<E, E2> mapper)
    {
        var result = await resultTask;
        return result.MapError(mapper);
    }

    public static async Task<Result<T2, E>> MapAsync<T, T2, E>(
        this Task<Result<T, E>> resultTask,
        Func<T, T2> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    public static async Task<Result<VoidValue, E>> MapVoidAsync<T, E>(
        this Task<Result<T, E>> resultTask)
    {
        var result = await resultTask;
        return result.MapVoid();
    }
}
