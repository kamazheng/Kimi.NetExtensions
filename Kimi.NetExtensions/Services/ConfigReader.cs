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

    public static T? GetSettings<T>()
    {
        var section = Configuration.GetSection(typeof(T).Name);
        return section.Get<T>();
    }
}