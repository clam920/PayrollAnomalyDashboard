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

// Centralizes exception -> HTTP status code mapping in one place, instead of
// a try/catch block duplicated in every Program.cs endpoint. Registered as
// the first thing in the pipeline (see Program.cs) so it wraps every
// downstream request.
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
            // FluentValidation's ValidationException carries one or more
            // field-level failures - worth a richer response shape than the
            // single "Error" string used for everything else below, so the
            // client can tell exactly which field(s) failed and why.
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

            // Anything we recognize (400/404/409) is an expected, handled
            // outcome - log it at Warning. A genuinely unmapped exception
            // (500) is unexpected and gets the full stack trace at Error.
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
        // Anything else is a bug, not a handled business-rule violation - the
        // client gets a generic message rather than a raw exception message
        // (which could leak internal detail), the real detail goes to logs.
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
    };
}

// Small extension method for a clean one-liner in Program.cs.
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}