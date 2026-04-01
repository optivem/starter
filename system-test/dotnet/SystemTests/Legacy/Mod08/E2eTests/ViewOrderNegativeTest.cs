using Dsl.Core.Shop;
using SystemTests.Legacy.Mod08.E2eTests.Base;
using Optivem.Testing;
using Xunit;

namespace SystemTests.Legacy.Mod08.E2eTests;

public class ViewOrderNegativeTest : BaseE2eTest
{
    public static IEnumerable<object[]> NonExistentOrderValues()
    {
        yield return new object[] { "NON-EXISTENT-ORDER-99999", "Order NON-EXISTENT-ORDER-99999 does not exist." };
        yield return new object[] { "NON-EXISTENT-ORDER-88888", "Order NON-EXISTENT-ORDER-88888 does not exist." };
        yield return new object[] { "NON-EXISTENT-ORDER-77777", "Order NON-EXISTENT-ORDER-77777 does not exist." };
    }

    [Theory]
    [ChannelData(ChannelType.UI, ChannelType.API)]
    [ChannelMemberData(nameof(NonExistentOrderValues))]
    public async Task ShouldNotBeAbleToViewNonExistentOrder(Channel channel, string orderNumber, string expectedErrorMessage)
    {
        await Scenario(channel)
            .When().ViewOrder()
                .WithOrderNumber(orderNumber)
            .Then().ShouldFail()
                .ErrorMessage(expectedErrorMessage);
    }
}











