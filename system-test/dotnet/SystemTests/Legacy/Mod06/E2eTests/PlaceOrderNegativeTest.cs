using Common;
using Driver.Port.External.Erp.Dtos;
using Dsl.Core.Shop;
using Driver.Port.Shop.Dtos;
using SystemTests.Commons.Constants;
using SystemTests.Legacy.Mod06.E2eTests.Base;
using SystemTests.Legacy.Mod06.E2eTests.Helpers;
using Optivem.Testing;
using SystemTests.Commons.Providers;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod06.E2eTests;

public class PlaceOrderNegativeTest : BaseE2eTest
{
    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldRejectOrderWithInvalidQuantity(Channel channel)
    {
        await SetChannelAsync(channel);

        var request = new PlaceOrderRequest 
        { 
            Sku = CreateUniqueSku(Defaults.SKU), 
            Quantity = "invalid-quantity", 

        };

        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must be an integer");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldRejectOrderWithNonExistentSku(Channel channel)
    {
        await SetChannelAsync(channel);

        var request = new PlaceOrderRequest 
        { 
            Sku = "NON-EXISTENT-SKU-12345", 
            Quantity = Defaults.QUANTITY, 

        };
        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "sku", "Product does not exist for SKU: NON-EXISTENT-SKU-12345");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldRejectOrderWithNegativeQuantity(Channel channel)
    {
        await SetChannelAsync(channel);

        var request = new PlaceOrderRequest 
        { 
            Sku = CreateUniqueSku(Defaults.SKU), 
            Quantity = "-10", 

        };
        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must be positive");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    public async Task ShouldRejectOrderWithZeroQuantity(Channel channel)
    {
        await SetChannelAsync(channel);

        var request = new PlaceOrderRequest 
        { 
            Sku = "ANOTHER-SKU-67890", 
            Quantity = "0", 

        };
        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must be positive");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    [ChannelClassData(typeof(EmptyArgumentsProvider))]
    public async Task ShouldRejectOrderWithEmptySku(Channel channel, string sku)
    {
        await SetChannelAsync(channel);

        var request = new PlaceOrderRequest 
        { 
            Sku = sku, 
            Quantity = Defaults.QUANTITY, 

        };
        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "sku", "SKU must not be empty");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    [ChannelClassData(typeof(EmptyArgumentsProvider))]
    public async Task ShouldRejectOrderWithEmptyQuantity(Channel channel, string emptyQuantity)
    {
        await SetChannelAsync(channel);

        var request = new PlaceOrderRequest 
        { 
            Sku = CreateUniqueSku(Defaults.SKU), 
            Quantity = emptyQuantity, 

        };
        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must not be empty");
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    [ChannelInlineData("3.5")]
    [ChannelInlineData("lala")]
    public async Task ShouldRejectOrderWithNonIntegerQuantity(Channel channel, string nonIntegerQuantity)
    {
        await SetChannelAsync(channel);

        var request = new PlaceOrderRequest 
        { 
            Sku = CreateUniqueSku(Defaults.SKU), 
            Quantity = nonIntegerQuantity, 

        };
        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must be an integer");
    }

    [Theory]
    [ChannelData(ChannelType.API)]
    public async Task ShouldRejectOrderWithNullQuantity(Channel channel)
    {
        await SetChannelAsync(channel);

        var request = new PlaceOrderRequest
        {
            Sku = CreateUniqueSku(Defaults.SKU),
            Quantity = null
        };
        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "quantity", "Quantity must not be empty");
    }

    [Theory]
    [ChannelData(ChannelType.API)]
    public async Task ShouldRejectOrderWithNullSku(Channel channel)
    {
        await SetChannelAsync(channel);

        var request = new PlaceOrderRequest 
        { 
            Sku = null, 
            Quantity = Defaults.QUANTITY, 

        };
        var result = await _shopDriver!.PlaceOrderAsync(request);

        result.ShouldBeFailure();
        result.Error.ShouldHaveMessageAndField("The request contains one or more validation errors", "sku", "SKU must not be empty");
    }

}














