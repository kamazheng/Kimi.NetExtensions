using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Kimi.NetExtensions.Services
{
    public static class Kimi
    {
        public static string[]? HostUris { get; set; }

        /// <summary>
        /// Init kimi extension. Set the current environment name if there's no system environmnet
        /// set. and set the license parameter by server.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="environmentName"></param>
        /// <param name="licenseServer"></param>
        public static void UseKimiExtension(this IServiceCollection services, string environmentName, string? licenseServer = null)
        {
            getHostUri();
            UseKimi(environmentName, licenseServer);
        }
        public static void UseKimiExtension(this WebApplicationBuilder builder, string environmentName, string? licenseServer = null)
        {
            getHostUri();
            UseKimi(environmentName, licenseServer);
        }

        private static void getHostUri()
        {
            HostUris = GetAllEnvironmentVariables("ASPNETCORE_URLS").Where(variable => !string.IsNullOrEmpty(variable))
                        .SelectMany(variable => variable.Split(';'))
                        .ToArray();
        }

        public static string[] GetAllEnvironmentVariables(string name)
        {
            var variables = new string?[]
            {
                Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine),
                Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User),
                Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process)
            };

            // Remove null items and cast to non-nullable string array
            var nonNullVariables = variables.Where(v => v != null).Select(v => v!).ToArray();

            // Return an empty array if all values are null
            return nonNullVariables.Length > 0 ? nonNullVariables : Array.Empty<string>();
        }


        private static void UseKimi(string environmentName, string? licenseServer)
        {
            var systemEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(systemEnvironment))
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environmentName);
                EnvironmentExtension.EnvironmentName = environmentName;
            }
            else
            {
                EnvironmentExtension.EnvironmentName = systemEnvironment;
            }
            LicenceHelper.SetParameterByServer(licenseServer);
        }

    }
}