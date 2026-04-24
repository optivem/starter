using Driver.Adapter.MyShop.Api.Client.Dtos.Errors;
using Driver.Port.MyShop.Dtos.Error;

namespace Driver.Adapter.MyShop.Api;

public static class SystemErrorMapper
{
    public static SystemError From(ProblemDetailResponse problemDetail)
    {
        var message = problemDetail.Detail ?? "Request failed";

        if (problemDetail.Errors != null && problemDetail.Errors.Count > 0)
        {
            var fieldErrors = problemDetail.Errors
                .Select(e => new SystemError.FieldError(e.Field ?? string.Empty, e.Message ?? string.Empty, e.Code))
                .ToList()
                .AsReadOnly();
            return SystemError.Of(message, fieldErrors);
        }

        return SystemError.Of(message);
    }
}
