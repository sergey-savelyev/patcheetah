using Microsoft.Extensions.DependencyInjection;
using System;

namespace Patcheetah.Swashbuckle
{
    public static class Extensions
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {

            });
        }
    }
}
