namespace Driver.Adapter.External.Erp.Client.Dtos.Error;

public class ExtErpErrorResponse
{
    public string? Message { get; set; }

    public static ExtErpErrorResponse From(string message)
    {
        return new ExtErpErrorResponse { Message = message };
    }
}

