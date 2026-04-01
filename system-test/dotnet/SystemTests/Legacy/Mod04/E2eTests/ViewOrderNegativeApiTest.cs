using Common;
using SystemTests.Legacy.Mod04.E2eTests.Base;
using Driver.Port.Shop.Dtos.Error;
using SystemTests.Legacy.Mod04.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod04.E2eTests;

public class ViewOrderNegativeApiTest : BaseE2eTest
{
    protected override Task SetShopClientAsync()
    {
        SetUpShopApiClient();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldNotBeAbleToViewNonExistentOrder()
    {
        var orderNumber = "NON-EXISTENT-ORDER-99999";
        var result = await _shopApiClient!.Orders().ViewOrderAsync(orderNumber);
        result.ShouldBeFailure();
        result.Error.Detail.ShouldBe("Order NON-EXISTENT-ORDER-99999 does not exist.");
    }
}













