using Driver.Port.External.Erp.Dtos;
using Dsl.Core.Shared;
using Shouldly;

namespace Dsl.Core.External.Erp.UseCases;

public class GetProductVerification : ResponseVerification<GetProductResponse>
{
    public GetProductVerification(GetProductResponse response, UseCaseContext context)
        : base(response, context)
    {
    }

    public GetProductVerification Sku(string skuParamAlias)
    {
        var expectedSku = Context.GetParamValue(skuParamAlias);
        var actualSku = Response.Sku;
        actualSku.ShouldBe(expectedSku, $"Expected SKU to be '{expectedSku}', but was '{actualSku}'");
        return this;
    }

    public GetProductVerification Price(decimal expectedPrice)
    {
        var actualPrice = Response.Price;
        actualPrice.ShouldBe(expectedPrice, $"Expected price to be {expectedPrice}, but was {actualPrice}");
        return this;
    }

    public GetProductVerification Price(string expectedPrice)
    {
        return Price(decimal.Parse(expectedPrice));
    }
}



