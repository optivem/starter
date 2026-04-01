namespace Dsl.Core.Shared;

public class ResponseVerification<TResponse>
{
    public ResponseVerification(TResponse response, UseCaseContext context)
    {
        Response = response;
        Context = context;
    }

    protected TResponse Response { get; }
    protected UseCaseContext Context { get; }
}
