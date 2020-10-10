using Microsoft.Extensions.DependencyInjection;

namespace Patcheetah.Swagger.NET21
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
