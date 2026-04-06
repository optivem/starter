namespace Driver.Adapter.External.Tax.Client.Dtos.Error;

public class ExtTaxErrorResponse
{
    public string? Message { get; set; }

    public static ExtTaxErrorResponse From(string message)
    {
        return new ExtTaxErrorResponse
        {
            Message = message
        };
    }

    public override string ToString()
    {
        return Message ?? string.Empty;
    }
}
