using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Patcheetah.Swagger;
using Patcheetah.SystemText;
using System;

namespace Patcheetah.TestApp31
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SAB.API", Version = "v1" });
                
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
            });
            services.AddPatchObjectSwaggerSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            PatchEngine.Init(cfg =>
            {
                cfg.EnableAttributes();
                cfg.SetPrePatchProcessingFunction(context =>
                {
                    if (context.NewValue is long lng)
                    {
                        return Convert.ToInt32(lng);
                    }

                    if (context.NewValue is string str && (context.OldValue?.GetType()?.IsEnum ?? false))
                    {
                        return Enum.Parse(context.OldValue.GetType(), str, true);
                    }

                    return context.NewValue;
                });
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SAB API V1");
            });
        }
    }
}
