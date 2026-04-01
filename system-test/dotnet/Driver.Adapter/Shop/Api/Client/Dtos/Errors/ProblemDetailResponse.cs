namespace Driver.Adapter.Shop.Api.Client.Dtos.Errors;

public class ProblemDetailResponse
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public string? Timestamp { get; set; }
    public List<FieldError>? Errors { get; set; }

    public class FieldError
    {
        public string? Field { get; set; }
        public string? Message { get; set; }
        public string? Code { get; set; }
        public object? RejectedValue { get; set; }
    }
}

