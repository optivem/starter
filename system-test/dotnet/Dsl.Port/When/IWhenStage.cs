using Dsl.Port.When.Steps;

namespace Dsl.Port.When;

public interface IWhenStage
{
    IWhenPlaceOrder PlaceOrder();

    IWhenCancelOrder CancelOrder();

    IWhenViewOrder ViewOrder();

    IWhenPublishCoupon PublishCoupon();

    IWhenBrowseCoupons BrowseCoupons();
}
