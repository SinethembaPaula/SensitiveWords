using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SensitiveWords.Api.Middleware
{
    public sealed class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found"),
                InvalidOperationException => (HttpStatusCode.UnprocessableEntity, "Unprocessable Request"),
                OperationCanceledException => (HttpStatusCode.ServiceUnavailable, "Request Cancelled"),
                _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
            };

            var problem = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
