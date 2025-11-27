using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace TopScorers.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiKeyAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string HeaderName = "x-api-key";
    private readonly bool requireKey;

    public ApiKeyAuthorizeAttribute(bool requireKey = true)
    {
        this.requireKey = requireKey;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!requireKey)
        {
            return Task.CompletedTask;
        }

        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiKeyAuthorizeAttribute>>();
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expectedKey = configuration["Security:ApiKey"];
        
        if (string.IsNullOrEmpty(expectedKey))
        {
            logger.LogWarning("API Key not configured. IP: {IpAddress}, Path: {Path}", 
                ipAddress, context.HttpContext.Request.Path);
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey))
        {
            logger.LogWarning("API Key missing from request. IP: {IpAddress}, Path: {Path}", 
                ipAddress, context.HttpContext.Request.Path);
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        if (!string.Equals(providedKey.ToString(), expectedKey, StringComparison.Ordinal))
        {
            logger.LogWarning("Invalid API Key provided. IP: {IpAddress}, Path: {Path}", 
                ipAddress, context.HttpContext.Request.Path);
            context.Result = new UnauthorizedResult();
        }

        return Task.CompletedTask;
    }
}

