using Microsoft.AspNetCore.Mvc;

namespace CommercialManagement.API.Helpers;

public static class ApiProblem
{
    public static ProblemDetails Create(string detail, string title, int status) =>
        new()
        {
            Title = title,
            Detail = detail,
            Status = status,
        };
}
