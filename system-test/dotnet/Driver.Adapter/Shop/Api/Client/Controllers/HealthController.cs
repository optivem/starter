using Common;

using Driver.Adapter.Shared.Client.Http;

using Driver.Adapter.Shop.Api.Client.Dtos.Errors;



namespace Driver.Adapter.Shop.Api.Client.Controllers;



public class HealthController

{

    private const string Endpoint = "/health";



    private readonly JsonHttpClient<ProblemDetailResponse> _httpClient;



    public HealthController(JsonHttpClient<ProblemDetailResponse> httpClient)

    {

        _httpClient = httpClient;

    }



    public Task<Result<VoidValue, ProblemDetailResponse>> CheckHealthAsync()

        => _httpClient.GetAsync(Endpoint);

}





