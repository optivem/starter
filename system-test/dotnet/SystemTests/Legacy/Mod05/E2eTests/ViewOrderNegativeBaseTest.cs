using Common;
using SystemTests.Legacy.Mod05.E2eTests.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod05.E2eTests;

public abstract class ViewOrderNegativeBaseTest : BaseE2eTest
{
    [Fact]
    public async Task ShouldNotBeAbleToViewNonExistentOrder()
    {
        var orderNumber = "NON-EXISTENT-ORDER-99999";
        var result = await _shopDriver!.ViewOrderAsync(orderNumber);
        result.ShouldBeFailure();
        result.Error.Message.ShouldBe("Order NON-EXISTENT-ORDER-99999 does not exist.");
    }
}











