using Common;
using SystemTests.Legacy.Mod06.Base;
using Shouldly;
using Xunit;

namespace SystemTests.Legacy.Mod06.SmokeTests.External;

public class ErpSmokeTest : BaseChannelDriverTest
{
    [Fact]
    public async Task ShouldBeAbleToGoToErp()
    {
        var result = await _erpDriver!.GoToErpAsync();
        result.ShouldBeSuccess();
    }
}










