using Common;
using SystemTests.Legacy.Mod06.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod06.SmokeTests.External;

public class TaxSmokeTest : BaseChannelDriverTest
{
    [Fact]
    public async Task ShouldBeAbleToGoToTax()
    {
        var result = await _taxDriver!.GoToTaxAsync();
        result.ShouldBeSuccess();
    }
}
