using Microsoft.Extensions.DependencyInjection;

namespace Kimi.NetExtensions.Services
{
    public static class Kimi
    {
        /// <summary>
        /// Init kimi extension.
        /// Set the current environment name and set the license parameter by server.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="environmentName"></param>
        /// <param name="licenseServer"></param>
        public static void UseKimiExtension(this IServiceCollection services, string environmentName, string? licenseServer = null)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environmentName);
            EnvironmentExtension.EnvironmentName = environmentName;
            LicenceHelper.SetParameterByServer(licenseServer);
        }
    }
}