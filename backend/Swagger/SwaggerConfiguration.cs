using System.Reflection;
using Microsoft.OpenApi.Models;

namespace VinhKhanhNarration.Api.Swagger;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddVinhKhanhSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Vinh Khanh Narration API",
                Version = "v1",
                Description = "RESTful API cho hệ thống thuyết minh tự động đa ngôn ngữ phố ẩm thực Vĩnh Khánh."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });
        return services;
    }
}
