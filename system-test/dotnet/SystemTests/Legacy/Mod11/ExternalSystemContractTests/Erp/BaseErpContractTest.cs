using SystemTests.Legacy.Mod11.ExternalSystemContractTests.Base;

namespace SystemTests.Legacy.Mod11.ExternalSystemContractTests.Erp;

public abstract class BaseErpContractTest : BaseExternalSystemContractTest
{
    [Fact]
    public async Task ShouldBeAbleToGetProduct()
    {
        (await Scenario()
            .Given().Product().WithSku("SKU-123").WithUnitPrice(12.0m)
            .Then().Product("SKU-123"))
            .HasSku("SKU-123")
            .HasPrice(12.0m);
    }
}










