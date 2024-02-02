using Microsoft.Extensions.Configuration;

public static class ConfigReader
{
    public static IConfigurationRoot Configuration => GetConfigReader();

    public static IConfigurationRoot GetConfigReader()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);

        var configuration = builder.Build();
        return configuration;
    }

    public static string? GetConfigValue(string key)
    {
        return GetConfigReader()[key];
    }

    public static string? GetConnectionString(string name)
    {
        return GetConfigReader().GetConnectionString(name);
    }
}