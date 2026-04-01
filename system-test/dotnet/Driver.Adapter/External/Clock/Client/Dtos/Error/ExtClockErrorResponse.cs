namespace Driver.Adapter.External.Clock.Client.Dtos.Error;

public class ExtClockErrorResponse
{
    public string? Message { get; set; }

    public static ExtClockErrorResponse From(string message)
    {
        return new ExtClockErrorResponse { Message = message };
    }

    public override string ToString()
    {
        return Message ?? string.Empty;
    }
}

