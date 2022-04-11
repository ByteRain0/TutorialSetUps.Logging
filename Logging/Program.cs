using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Host.UseSerilog((ctx, cfg) => 
        cfg.ReadFrom.Configuration(ctx.Configuration));
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    
    // Create visibility by keeping infrastructure code similar across solutions
    app.UseMiddleware<RequestPerformanceMiddleware>();

    // Log at the proper level
    app.MapGet("/logAtDifferentLevels", (ILogger<Program> logger) =>
    {
        logger.LogTrace("Detailed messages with sensitive app data");
        logger.LogDebug("Useful for the development environment");
        logger.LogInformation("General messages");
        logger.LogWarning("Unexpected events that can be “gracefully” handled by the code");
        logger.LogError("General exceptions and failures");
        logger.LogCritical("Failures that require immediate attention");
    });

    // Log just enough meaningful information
    // Log in a structured manner and keep it consistent
    app.MapGet("/logStructuredMessageAboutError", (ILogger<Program> logger) =>
    {
        logger.LogError("Error : '{ErrorCode}' encountered", ErrorMessages.BeaverErrorCode);
    });
    
    // Expect your logs to be read out of context
    app.MapPost("/remediationStep", (int parameter, ILogger<Program> logger) =>
    {
        if (parameter < 0)
        {
            logger.LogError("Error : '{ErrorCode}' encountered while processing request " +
                            "X for user Y.  ... some remediation steps ...",
                ErrorMessages.BeaverErrorCode);
            
            return Results.Problem(
                detail: "Parameter cannot be less than 0.",
                title: "Failure executing request X"
                );
        }
        //... do some other logic.
        return Results.Ok();
    });
    
    // Emulate long running requests to trigger the middleware that will log warnings about issues we have.
    app.MapGet("/longRun", () =>
    {
        Thread.Sleep(500);
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}