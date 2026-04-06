using Dsl.Port;

namespace SystemTests.Latest.ExternalSystemContractTests.Tax;

public class TaxRealContractTest : BaseTaxContractTest
{
    protected override ExternalSystemMode? FixedExternalSystemMode => ExternalSystemMode.Real;
}
