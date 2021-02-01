using ApiMonitor.Configuration;
using ApiMonitor.Helper;
using ApiMonitor.Helper.ApiKeyAuthorization;
using ApiMonitor.Services;
using ApiMonitor.Services.Implements;
using ApiMonitor.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiMonitor
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
            services.AddCors();
            services.AddControllers();

            #region config
            // config file deserialize
            var configApiKey = new ApiKeyCfg();
            Configuration.Bind("ApiKeyCfg", configApiKey);
            services.AddSingleton(configApiKey);

            var configPandoraApi = new ApiPandora();
            Configuration.Bind("ApiPandora", configPandoraApi);
            services.AddSingleton(configPandoraApi);
            #endregion

            #region api key authentication
            // api key authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
            })
            .AddApiKeySupport(options => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.WebIt, policy => policy.Requirements.Add(new WebItRequirement()));
            });

            services.AddScoped<IAuthorizationHandler, WebItAuthorizationHandler>();
            services.AddScoped<IGetApiKey, GetApiKey>();

            #endregion

            #region basic authentication
            // configure basic authentication 
            //services.AddAuthentication("BasicAuthentication")
            //    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            // configure DI for application services
            //services.AddScoped<IUserService, UserService>();
            #endregion

            #region DI
            // DI
            services.AddScoped<IWebItStrategy, WebItServiceStrategy>();
            services.AddScoped<IMonitor, PandoraMonitor>();
            services.AddScoped<IMonitor, OtherMonitor>();
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ////if (env.IsDevelopment())
            ////{
            ////    app.UseDeveloperExceptionPage();
            ////}

            //app.UseHttpsRedirection();

            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
