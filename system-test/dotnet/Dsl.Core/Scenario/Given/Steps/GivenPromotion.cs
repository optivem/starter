using Dsl.Core.Scenario.Given;
using Dsl.Port.Given.Steps;
using Driver.Adapter;
using static Dsl.Core.Gherkin.GherkinDefaults;

namespace Dsl.Core.Gherkin.Given;

public class GivenPromotion : BaseGiven, IGivenPromotion
{
    private bool _promotionActive;
    private string? _discount;

    public GivenPromotion(GivenStage givenClause) : base(givenClause)
    {
        _promotionActive = DefaultPromotionActive;
        _discount = DefaultPromotionDiscount;
    }

    public GivenPromotion WithActive(bool promotionActive)
    {
        _promotionActive = promotionActive;
        return this;
    }

    IGivenPromotion IGivenPromotion.WithActive(bool promotionActive) => WithActive(promotionActive);

    public GivenPromotion WithDiscount(decimal discount) => WithDiscount(discount.ToString());

    public GivenPromotion WithDiscount(string? discount)
    {
        _discount = discount;
        return this;
    }

    IGivenPromotion IGivenPromotion.WithDiscount(decimal discount) => WithDiscount(discount);
    IGivenPromotion IGivenPromotion.WithDiscount(string? discount) => WithDiscount(discount);

    internal override async Task Execute(UseCaseDsl app)
    {
        (await app.Erp().ReturnsPromotion()
            .WithActive(_promotionActive)
            .WithDiscount(_discount)
            .Execute())
            .ShouldSucceed();
    }
}
