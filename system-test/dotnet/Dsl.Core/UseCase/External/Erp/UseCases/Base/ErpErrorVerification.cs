using Driver.Port.External.Erp.Dtos.Error;
using Dsl.Core.Shared;
using Shouldly;

namespace Dsl.Core.External.Erp.UseCases.Base;

public class ErpErrorVerification : ResponseVerification<ErpErrorResponse>
{
    public ErpErrorVerification(ErpErrorResponse error, UseCaseContext context)
        : base(error, context)
    {
    }

    public ErpErrorVerification ErrorMessage(string expectedMessage)
    {
        var expandedExpectedMessage = Context.ExpandAliases(expectedMessage);
        var error = Response;
        var errorMessage = error.Message;

        errorMessage.ShouldBe(expandedExpectedMessage,
            $"Expected error message: '{expandedExpectedMessage}', but got: '{errorMessage}'");

        return this;
    }
}



