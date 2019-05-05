using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using Swashbuckle.AspNetCore.Swagger;
using MySql.Data.MySqlClient;

namespace QuartzAdmin.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<IScheduler>(provider =>
            {
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX,Quartz" },
                    {"quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.MySQLDelegate"},
                     { "quartz.jobStore.tablePrefix", "QRTZ_" },
                    { "quartz.serializer.type", "json" },

                    { "quartz.jobStore.dataSource", "default" },
                    { "quartz.dataSource.default.connectionString", Configuration.GetConnectionString("Default")},
                    { "quartz.dataSource.default.provider","MySql" }
                };
                var factory =  new StdSchedulerFactory(props);
                var task = factory.GetScheduler();
                task.Wait();
                return task.Result;
            });

            //配置Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info()
                {
                    Title = $"作业调度接口文档",
                    Version = "V1.0",
                    Description = "作业调度接口文档"
                });

                options.IncludeXmlComments(@"QuartzAdmin.Api.xml", true);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(configure =>
            {
                configure.AllowAnyHeader();
                configure.AllowAnyOrigin();
                configure.AllowAnyMethod();
                configure.AllowCredentials();
            });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "api/[controller]/[action]"
                );
            });

            //配置Swagger
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DocumentTitle = "作业调度接口文档";
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); //控制显示折叠
                options.DisplayRequestDuration();  //显示请求时间
                options.ShowExtensions();
                options.SwaggerEndpoint($"/swagger/v1/swagger.json", "V1.0");
            });

            var conn = MySqlClientFactory.Instance.CreateConnection();
            conn.ConnectionString = Configuration.GetConnectionString("Default");
            try
            {
                conn.Open();
                conn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
