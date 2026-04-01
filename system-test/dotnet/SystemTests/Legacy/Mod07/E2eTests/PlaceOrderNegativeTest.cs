using Common;
using Dsl.Core.Shop;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod07.E2eTests.Base;
using Optivem.Testing;
using SystemTests.Commons.Providers;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod07.E2eTests;

public class PlaceOrderNegativeTest : BaseE2eTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldRejectOrderWithInvalidQuantity(Channel channel)
    {
        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .Sku(Defaults.SKU)
            .Quantity("invalid-quantity")
            .Execute())
            .ShouldFail()
            .ErrorMessage("The request contains one or more validation errors")
            .FieldErrorMessage("quantity", "Quantity must be an integer");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldRejectOrderWithNonExistentSku(Channel channel)
    {
        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .Sku("NON-EXISTENT-SKU-12345")
            .Quantity(Defaults.QUANTITY)
            .Execute())
            .ShouldFail()
            .ErrorMessage("The request contains one or more validation errors")
            .FieldErrorMessage("sku", "Product does not exist for SKU: NON-EXISTENT-SKU-12345");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldRejectOrderWithNegativeQuantity(Channel channel)
    {
        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .Sku(Defaults.SKU)
            .Quantity(-10)
            .Execute())
            .ShouldFail()
            .ErrorMessage("The request contains one or more validation errors")
            .FieldErrorMessage("quantity", "Quantity must be positive");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldRejectOrderWithZeroQuantity(Channel channel)
    {
        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .Sku("ANOTHER-SKU-67890")
            .Quantity(0)
            .Execute())
            .ShouldFail()
            .ErrorMessage("The request contains one or more validation errors")
            .FieldErrorMessage("quantity", "Quantity must be positive");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    [ChannelClassData(typeof(EmptyArgumentsProvider))]
    public async Task ShouldRejectOrderWithEmptySku(Channel channel, string sku)
    {
        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .Sku(sku)
            .Quantity(Defaults.QUANTITY)
            .Execute())
            .ShouldFail()
            .ErrorMessage("The request contains one or more validation errors")
            .FieldErrorMessage("sku", "SKU must not be empty");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    [ChannelClassData(typeof(EmptyArgumentsProvider))]
    public async Task ShouldRejectOrderWithEmptyQuantity(Channel channel, string emptyQuantity)
    {
        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .Sku(Defaults.SKU)
            .Quantity(emptyQuantity)
            .Execute())
            .ShouldFail()
            .ErrorMessage("The request contains one or more validation errors")
            .FieldErrorMessage("quantity", "Quantity must not be empty");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    [ChannelInlineData("3.5")]
    [ChannelInlineData("lala")]
    public async Task ShouldRejectOrderWithNonIntegerQuantity(Channel channel, string nonIntegerQuantity)
    {
        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .Sku(Defaults.SKU)
            .Quantity(nonIntegerQuantity)
            .Execute())
            .ShouldFail()
            .ErrorMessage("The request contains one or more validation errors")
            .FieldErrorMessage("quantity", "Quantity must be an integer");
    }

    [Theory]
    [ChannelData(ChannelType.API)]
    public async Task ShouldRejectOrderWithNullQuantity(Channel channel)
    {
        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .Sku(Defaults.SKU)
            .Quantity(null)
            .Execute())
            .ShouldFail()
            .ErrorMessage("The request contains one or more validation errors")
            .FieldErrorMessage("quantity", "Quantity must not be empty");
    }

    [Theory]
    [ChannelData(ChannelType.API)]
    public async Task ShouldRejectOrderWithNullSku(Channel channel)
    {
        var shop = await _app.Shop(channel);
        (await shop.PlaceOrder()
            .Sku(null)
            .Quantity(Defaults.QUANTITY)
            .Execute())
            .ShouldFail()
            .ErrorMessage("The request contains one or more validation errors")
            .FieldErrorMessage("sku", "SKU must not be empty");
    }
}











