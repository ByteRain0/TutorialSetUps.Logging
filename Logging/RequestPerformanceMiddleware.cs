using System.Diagnostics;

public class RequestPerformanceMiddleware
{
    private readonly RequestDelegate _next;

    public RequestPerformanceMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// PoC middleware component.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    public async Task InvokeAsync(HttpContext context, ILogger<RequestPerformanceMiddleware> logger)
    {
        var stopwatch = new Stopwatch();
        
        stopwatch.Start();
        
        await _next.Invoke(context);
        
        stopwatch.Stop();
        
        if (stopwatch.Elapsed.Milliseconds < 200) return;
        
        logger.LogWarning("{LongRunningRequestCode} request {RequestPath} is taking {ElapsedTime} mls", 
            ErrorMessages.LongRunningRequest, context.Request.Path, stopwatch.Elapsed.Milliseconds);
    }
    
}