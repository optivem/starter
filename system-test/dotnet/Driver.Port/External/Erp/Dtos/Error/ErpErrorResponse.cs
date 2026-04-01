namespace Driver.Port.External.Erp.Dtos.Error;

public class ErpErrorResponse
{
    public string? Message { get; set; }

    public override string ToString()
    {
        return Message ?? string.Empty;
    }
}

