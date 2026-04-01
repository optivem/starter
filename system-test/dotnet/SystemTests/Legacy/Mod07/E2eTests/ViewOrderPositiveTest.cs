using Common;
using Dsl.Core.Shop;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod07.E2eTests.Base;
using Optivem.Testing;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod07.E2eTests;

public class ViewOrderPositiveTest : BaseE2eTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldViewPlacedOrder(Channel channel)
    {
        (await _app.Erp()
            .ReturnsProduct()
            .Sku(Defaults.SKU)
            .UnitPrice(25.00m)
            .Execute())
            .ShouldSucceed();

        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .OrderNumber(Defaults.ORDER_NUMBER)
            .Sku(Defaults.SKU)
            .Quantity(4)
            .Execute())
            .ShouldSucceed();

        (await shop.ViewOrder()
            .OrderNumber(Defaults.ORDER_NUMBER)
            .Execute())
            .ShouldSucceed()
            .OrderNumber(Defaults.ORDER_NUMBER)
            .Sku(Defaults.SKU)
            .Quantity(4)
            .UnitPrice(25.00m)
            .TotalPrice(100.00m)
            .Status(OrderStatus.Placed);
    }
}













