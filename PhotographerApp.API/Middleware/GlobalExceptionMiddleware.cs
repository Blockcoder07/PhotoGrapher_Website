using System.Net;
using System.Text.Json;
using PhotographerApp.Core.DTOs;

namespace PhotographerApp.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse
        {
            Success = false,
            Message = "An error occurred while processing your request.",
            Errors = new List<string>()
        };

        switch (exception)
        {
            case ArgumentException ae:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = ae.Message;
                response.Errors.Add(ae.ParamName ?? "Unknown parameter");
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Unauthorized access";
                break;

            case KeyNotFoundException kne:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = kne.Message;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Errors.Add(exception.Message);
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}
