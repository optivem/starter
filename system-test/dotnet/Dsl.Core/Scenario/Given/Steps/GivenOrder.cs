using Common;
using Dsl.Port.Given.Steps;
using Dsl.Core.Scenario.Given;
using Driver.Port.Shop.Dtos;
using static Dsl.Core.Gherkin.GherkinDefaults;

namespace Dsl.Core.Gherkin.Given;

public class GivenOrder : BaseGiven, IGivenOrder
{
    private string? _orderNumber;
    private string? _sku;
    private string? _quantity;
    private OrderStatus _status;

    public GivenOrder(GivenStage givenClause) : base(givenClause)
    {
        WithOrderNumber(DefaultOrderNumber);
        WithSku(DefaultSku);
        WithQuantity(DefaultQuantity);
        WithStatus(DefaultOrderStatus);
    }

    public GivenOrder WithOrderNumber(string orderNumber)
    {
        _orderNumber = orderNumber;
        return this;
    }

    IGivenOrder IGivenOrder.WithOrderNumber(string orderNumber) => WithOrderNumber(orderNumber);

    public GivenOrder WithSku(string? sku)
    {
        _sku = sku;
        return this;
    }

    IGivenOrder IGivenOrder.WithSku(string? sku) => WithSku(sku);

    public GivenOrder WithQuantity(string? quantity)
    {
        _quantity = quantity;
        return this;
    }

    IGivenOrder IGivenOrder.WithQuantity(string? quantity) => WithQuantity(quantity);

    public GivenOrder WithQuantity(int? quantity)
    {
        return WithQuantity(Converter.FromInteger(quantity));
    }

    IGivenOrder IGivenOrder.WithQuantity(int? quantity) => WithQuantity(quantity);

    public GivenOrder WithStatus(OrderStatus status)
    {
        _status = status;
        return this;
    }

    IGivenOrder IGivenOrder.WithStatus(OrderStatus status) => WithStatus(status);

    internal override async Task Execute(UseCaseDsl app)
    {
        var shop = await app.Shop(Channel);

        (await shop.PlaceOrder()
            .OrderNumber(_orderNumber)
            .Sku(_sku)
            .Quantity(_quantity)
            .Execute())
            .ShouldSucceed();
    }
}



