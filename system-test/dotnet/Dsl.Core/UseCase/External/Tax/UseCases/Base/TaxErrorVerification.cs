using Driver.Port.External.Tax.Dtos.Error;
using Dsl.Core.Shared;
using Shouldly;

namespace Dsl.Core.External.Tax.UseCases.Base;

public class TaxErrorVerification : ResponseVerification<TaxErrorResponse>
{
    public TaxErrorVerification(TaxErrorResponse error, UseCaseContext context)
        : base(error, context)
    {
    }

    public TaxErrorVerification ErrorMessage(string expectedMessage)
    {
        var expandedExpectedMessage = Context.ExpandAliases(expectedMessage);
        var error = Response;
        var errorMessage = error.Message;

        errorMessage.ShouldBe(expandedExpectedMessage,
            $"Expected error message: '{expandedExpectedMessage}', but got: '{errorMessage}'");

        return this;
    }
}
