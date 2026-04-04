using Dsl.Port.Given.Steps.Base;

namespace Dsl.Port.Given.Steps;

public interface IGivenPromotion : IGivenStep
{
    IGivenPromotion WithActive(bool promotionActive);
    IGivenPromotion WithDiscount(decimal discount);
    IGivenPromotion WithDiscount(string? discount);
}
