using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Patcheetah.JsonNET;
using Patcheetah.Swagger;
using System;

namespace Patcheetah.TestApp21
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
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "Patcheetah.API", Version = "v1" });
            });
            
            services.AddPatchObjectSwaggerSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Patcheetah API V1");
            });
        }
    }
}
