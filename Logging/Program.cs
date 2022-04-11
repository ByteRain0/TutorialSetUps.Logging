using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();

    app.MapGet("/logAtDifferentLevels", (ILogger<Program> logger) =>
    {
        logger.LogTrace("Detailed messages with sensitive app data.");
        logger.LogDebug("Useful for the development environment.");
        logger.LogInformation("General messages.");
        logger.LogWarning("Unexpected events that can be “gracefully” handled by the code.");
        logger.LogError("General exceptions and failures.");
        logger.LogCritical("Failures that require immediate attention");
    });

    app.MapGet("/logStructuredMessageAboutError", (ILogger<Program> logger) =>
    {
        logger.LogError("Error : '{ErrorCode}' encountered.", ErrorMessages.BeaverErrorCode);
    });

    app.MapGet("/throwException", () =>
    {
        throw new InvalidOperationException("Some message");
    });

    app.MapPost("/remediationStep", (int parameter, ILogger<Program> logger) =>
    {
        if (parameter < 0)
        {
            return Results.Problem(
                detail: "Parameter cannot be less than 0. ... some remediation steps ...",
                title: "Failure executing request X");
        }
        //... do some other logic.
        return Results.Ok();
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