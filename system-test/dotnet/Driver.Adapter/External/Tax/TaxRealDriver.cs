using Common;
using Driver.Adapter.External.Tax.Client;
using Driver.Port.External.Tax.Dtos;
using Driver.Port.External.Tax.Dtos.Error;

namespace Driver.Adapter.External.Tax;

public class TaxRealDriver : BaseTaxDriver<TaxRealClient>
{
    public TaxRealDriver(string baseUrl) : base(new TaxRealClient(baseUrl))
    {
    }

    public override Task<Result<VoidValue, TaxErrorResponse>> ReturnsTaxRateAsync(ReturnsTaxRateRequest request)
    {
        // No-op for real driver - data already exists in real service
        return Task.FromResult(Result.Success<TaxErrorResponse>());
    }
}
