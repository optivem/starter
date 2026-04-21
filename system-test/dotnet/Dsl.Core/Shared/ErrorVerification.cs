using Shouldly;

namespace Dsl.Core.Shared;

public class ErrorVerification<TError> : ResponseVerification<TError>
{
    private readonly Func<TError, string?> _messageSelector;

    public ErrorVerification(TError error, UseCaseContext context, Func<TError, string?> messageSelector)
        : base(error, context)
    {
        _messageSelector = messageSelector;
    }

    public ErrorVerification<TError> ErrorMessage(string expectedMessage)
    {
        var expandedExpectedMessage = Context.ExpandAliases(expectedMessage);
        var errorMessage = _messageSelector(Response);

        errorMessage.ShouldBe(expandedExpectedMessage,
            $"Expected error message: '{expandedExpectedMessage}', but got: '{errorMessage}'");

        return this;
    }
}
