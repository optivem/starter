using Dsl.Core.Shared;
using Shouldly;
using Driver.Port.Shop.Dtos;

namespace Dsl.Core.Shop.UseCases;

public class PlaceOrderVerification : ResponseVerification<PlaceOrderResponse>
{
    public PlaceOrderVerification(PlaceOrderResponse response, UseCaseContext context)
        : base(response, context)
    {
    }

    public PlaceOrderVerification OrderNumber(string orderNumberResultAlias)
    {
        var expectedOrderNumber = Context.GetResultValue(orderNumberResultAlias);
        var actualOrderNumber = Response.OrderNumber;

        actualOrderNumber.ShouldBe(expectedOrderNumber, $"Expected order number to be '{expectedOrderNumber}', but was '{actualOrderNumber}'");

        return this;
    }

    public PlaceOrderVerification OrderNumberStartsWith(string prefix)
    {
        Response.OrderNumber.ShouldStartWith(prefix, Case.Sensitive, $"Expected order number to start with '{prefix}', but was '{Response.OrderNumber}'");
        return this;
    }
}



