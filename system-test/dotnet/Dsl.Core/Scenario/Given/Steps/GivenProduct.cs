using Common;
using Dsl.Port.Given.Steps;
using Dsl.Core.Scenario.Given;
using static Dsl.Core.Gherkin.GherkinDefaults;

namespace Dsl.Core.Gherkin.Given;

public class GivenProduct : BaseGiven, IGivenProduct
{
    private string? _sku;
    private string? _unitPrice;

    public GivenProduct(GivenStage givenClause)
        : base(givenClause)
    {
        WithSku(DefaultSku);
        WithUnitPrice(DefaultUnitPrice);
    }

    public GivenProduct WithSku(string? sku)
    {
        _sku = sku;
        return this;
    }

    IGivenProduct IGivenProduct.WithSku(string? sku) => WithSku(sku);

    public GivenProduct WithUnitPrice(string? unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    IGivenProduct IGivenProduct.WithUnitPrice(string? unitPrice) => WithUnitPrice(unitPrice);

    public GivenProduct WithUnitPrice(decimal? unitPrice)
    {
        return WithUnitPrice(Converter.FromDecimal(unitPrice));
    }

    IGivenProduct IGivenProduct.WithUnitPrice(decimal? unitPrice) => WithUnitPrice(unitPrice);

    internal override async Task Execute(UseCaseDsl app)
    {
        (await app.Erp().ReturnsProduct()
            .Sku(_sku)
            .UnitPrice(_unitPrice)
            .Execute())
            .ShouldSucceed();
    }
}


