using SystemTests.Legacy.Mod03.Base;
using Xunit;

namespace SystemTests.Legacy.Mod03.SmokeTests.System;

public class ShopUiSmokeTest : BaseRawTest
{
    public override async Task InitializeAsync()
    {
        await SetUpShopBrowserAsync();
    }

    [Fact]
    public async Task ShouldBeAbleToGoToShop()
    {
        var response = await shopUiPage!.GotoAsync(_configuration.ShopUiBaseUrl);

        Assert.Equal(200, response!.Status);

        var contentType = response.Headers["content-type"];
        Assert.NotNull(contentType);
        Assert.Contains("text/html", contentType);

        var pageContent = await shopUiPage.ContentAsync();
        Assert.Contains("<html", pageContent);
        Assert.Contains("</html>", pageContent);
    }
}











