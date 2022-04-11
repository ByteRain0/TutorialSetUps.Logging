using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

public static class SerilogInfrastructure
{
    public static IConfigurationRoot AddSerilog(this IConfigurationRoot root)
    {
        //Leaving all the configuration to the app settings file.
        //Easier for infra guys to change set ups.
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(root)
            .CreateLogger();

        return root;
    }

    public static void RunWithSerilogEnabled(this IHostBuilder builder)
    {
        try
        {
            Log.Information("Application starting");
            builder.Build().Run();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "The application encountered a fatal exception");
        }
        finally
        {
            //Make sure all the pending log messages are written before the log closes.
            Log.CloseAndFlush();
        }
    }
}
