using Common;
using Driver.Adapter.External.Erp.Client.Dtos;
using Driver.Adapter.External.Erp.Client.Dtos.Error;

namespace Driver.Adapter.External.Erp.Client;

public class ErpRealClient : BaseErpClient
{
    private const string ProductsEndpoint = "/api/products";

    public ErpRealClient(string baseUrl) : base(baseUrl)
    {
    }


    public Task<Result<VoidValue, ExtErpErrorResponse>> CreateProductAsync(ExtCreateProductRequest request)
        => HttpClient.PostAsync(ProductsEndpoint, request);
}

