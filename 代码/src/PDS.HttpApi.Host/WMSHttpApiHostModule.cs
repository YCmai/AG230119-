using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PDS.EntityFrameworkCore;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Json;
using Volo.Abp.Identity;
using WMS.Identity;
using System.Threading.Tasks;
using Volo.Abp.PermissionManagement.HttpApi;
using Spark;

namespace WMS
{
    [DependsOn(

        typeof(StorageModule.WMSStorageModule),
        typeof(MaterialModule.WMSMaterialModule),
        typeof(LineCallInputModule.WMSLineCallInputModule),
        typeof(HBTaskModule.HBTaskModule),
        typeof(WMS.LineCallProcessTaskModule.WMSLineCallProcessTaskModule),
        typeof(WMSEntityFrameworkCoreModule),
        typeof(WMS.BarCodeModule.WMSBarCodeModule),

        typeof(WMSIdentityModule),
        typeof(AbpIdentityHttpApiModule),
        typeof(AbpAccountWebModule),
        typeof(AbpPermissionManagementHttpApiModule),
        typeof(AciModule.AciModule),

        typeof(AbpIdentityApplicationModule),
        typeof(AbpAutofacModule),
        typeof(AbpAspNetCoreMultiTenancyModule),
        //typeof(AbpAspNetCoreMvcUiBasicThemeModule),
        typeof(AbpAspNetCoreSerilogModule),
        typeof(AbpSwashbuckleModule),
        typeof(RcsModule),
        typeof(AbpIdentityAspNetCoreModule)
    )]
    public class WMSHttpApiHostModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostingEnvironment = context.Services.GetHostingEnvironment();
            //防止攻击(屏蔽)
            Configure<AbpAntiForgeryOptions>(opt =>
            {
                opt.AutoValidate = false;
            });
            Configure<MvcNewtonsoftJsonOptions>(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";//对类型为DateTime的生效
            });
            Configure<AbpJsonOptions>(options =>
            options.DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss" //对类型为DateTimeOffset生效
            );
            ConfigureIdentity(context);
            ConfigureLocalization();
            ConfigureCors(context, configuration);
            ConfigureSwaggerServices(context, configuration);
        }
        private void ConfigureIdentity(ServiceConfigurationContext context)
        {
            context.Services.ConfigureApplicationCookie(options =>
            {
                //禁止未登录的时候跳转到Account/Login,直接返回401
                options.Events.OnRedirectToLogin = (context) =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                //禁止未授权时候跳转到Access/Deline,直接返回403
                options.Events.OnRedirectToAccessDenied = (context) =>
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
                //鉴于Chrome的安全策略,必须设置该值才能保存当前的Cookie()
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None;
                //options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;

            });
        }

        private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddAbpSwaggerGenWithOAuth(
                configuration["AuthServer:Authority"],
                new Dictionary<string, string>
                {
                    {"PDS", "PDS API"}
                },
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo {Title = "PDS API", Version = "v1"});
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);
                });
        }

        private void ConfigureLocalization()
        {
            Configure<AbpLocalizationOptions>(options =>
            {
                options.Languages.Add(new LanguageInfo("en", "en", "English"));
                options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));
            });
        }

        private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddCors(options =>
            {
                options.AddDefaultPolicy( builder =>
                {
                    builder
                        .WithOrigins(
                            configuration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .WithAbpExposedHeaders()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            app.UseAbpRequestLocalization();

            //if (!env.IsDevelopment())
            //{
            //    //app.UseErrorPage();
            //}

            //app.UseCorrelationId();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            //app.UseJwtTokenMiddleware();

            //app.UseMultiTenancy();
        
            app.UseUnitOfWork();
            //app.UseIdentityServer();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseAbpSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PDS API");

                var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
                c.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
                c.OAuthClientSecret(configuration["AuthServer:SwaggerClientSecret"]);
                c.OAuthScopes("PDS");
            });

            app.UseAuditing();
            app.UseAbpSerilogEnrichers();
            app.UseConfiguredEndpoints();
        }
    }
}
