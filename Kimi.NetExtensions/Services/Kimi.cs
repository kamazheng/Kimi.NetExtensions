using Microsoft.Extensions.DependencyInjection;

namespace Kimi.NetExtensions.Services
{
    public static class Kimi
    {
        /// <summary>
        /// Start to use Kimi Extensions
        /// </summary>
        /// <param name="services">
        /// </param>
        public static void UseKimiExtension(this IServiceCollection services)
        {
            LicenceHelper.SetParameterByServer();
        }
    }
}