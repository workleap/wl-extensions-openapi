internal static class HttpResultsStatusCodeTypeHelpers
{
    public static Dictionary<string, int> HttpResultTypeToStatusCodes { get; } = new()
    {
        {"Microsoft.AspNetCore.Http.HttpResults.Ok", 200},
        {"Microsoft.AspNetCore.Http.HttpResults.Ok`1", 200},
        {"Microsoft.AspNetCore.Http.HttpResults.Created", 201},
        {"Microsoft.AspNetCore.Http.HttpResults.Created`1", 201},
        {"Microsoft.AspNetCore.Http.HttpResults.CreatedAtRoute", 201},
        {"Microsoft.AspNetCore.Http.HttpResults.CreatedAtRoute`1", 201},
        {"Microsoft.AspNetCore.Http.HttpResults.Accepted", 202},
        {"Microsoft.AspNetCore.Http.HttpResults.Accepted`1", 202},
        {"Microsoft.AspNetCore.Http.HttpResults.AcceptedAtRoute", 202},
        {"Microsoft.AspNetCore.Http.HttpResults.AcceptedAtRoute`1", 202},
        {"Microsoft.AspNetCore.Http.HttpResults.NoContent", 204},
        {"Microsoft.AspNetCore.Http.HttpResults.BadRequest", 400},
        {"Microsoft.AspNetCore.Http.HttpResults.BadRequest`1", 400},
        {"Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult", 401},
        {"Workleap.Extensions.OpenAPI.TypedResult.Forbidden", 403},
        {"Workleap.Extensions.OpenAPI.TypedResult.Forbidden`1", 403},
        {"Microsoft.AspNetCore.Http.HttpResults.NotFound", 404},
        {"Microsoft.AspNetCore.Http.HttpResults.NotFound`1", 404},
        {"Microsoft.AspNetCore.Http.HttpResults.Conflict", 409},
        {"Microsoft.AspNetCore.Http.HttpResults.Conflict`1", 409},
        {"Microsoft.AspNetCore.Http.HttpResults.UnprocessableEntity", 422},
        {"Microsoft.AspNetCore.Http.HttpResults.UnprocessableEntity`1", 422},
        // Will be Supported in .NET 9
        {"Microsoft.AspNetCore.Http.HttpResults.InternalServerError", 500},
        {"Microsoft.AspNetCore.Http.HttpResults.InternalServerError`1", 500},
        // Workleap's definition of the InternalServerError type result for other .NET versions
        {"Workleap.Extensions.OpenAPI.TypedResult.InternalServerError", 500},
        {"Workleap.Extensions.OpenAPI.TypedResult.InternalServerError`1", 500},
    };
}