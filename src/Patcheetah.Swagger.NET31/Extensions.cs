using Microsoft.Extensions.DependencyInjection;
using Patcheetah.Swagger.NET31;

namespace Patcheetah.Swagger
{
    public static class Extensions
    {
        public static void AddPatchObjectSwaggerSupport(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.OperationFilter<PatchObjectOperationFilter>();
                options.SchemaFilter<AttrsResolvingSchemaFilter>();
            });
        }
    }
}
