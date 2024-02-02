using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Kimi.NetExtensions.Services
{
    /// <summary>
    /// Jwt security swagger service
    /// </summary>
    public static class JwtSwagger
    {
        /// <summary>
        /// Add Jwt security swagger service with project documentation included.
        /// </summary>
        /// <param name="services">
        /// </param>

        public static void AddJwtSwagger(this IServiceCollection services)
        {
            services.AddMvc();
            services.AddSwaggerGen(config =>
            {
                AddSwaggerDocumentation(config);
                config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Scheme = "bearer",
                    Description = "Please insert JWT token into field"
                });

                config.AddSecurityRequirement(new OpenApiSecurityRequirement
                     {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    });
            });
        }

        static void AddSwaggerDocumentation(SwaggerGenOptions o)
        {
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        }
    }
}