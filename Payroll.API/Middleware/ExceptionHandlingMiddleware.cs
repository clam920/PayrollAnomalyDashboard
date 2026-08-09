using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Payroll.Application.Exceptions;

namespace Payroll.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException validationEx)
        {
            _logger.LogWarning("Validation failed: {Errors}",
                string.Join("; ", validationEx.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = validationEx.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage });
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { Errors = errors }));
        }
        catch (Exception ex)
        {
            var (statusCode, message) = MapException(ex);

            if (statusCode == StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning("{ExceptionType} handled as {StatusCode}: {Message}", ex.GetType().Name, statusCode, message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { Error = message }));
        }
    }

    private static (int StatusCode, string Message) MapException(Exception ex) => ex switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
        ArgumentException => (StatusCodes.Status400BadRequest, ex.Message),
        InvalidOperationException => (StatusCodes.Status409Conflict, ex.Message),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
    };
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}