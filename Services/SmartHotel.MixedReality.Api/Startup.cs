using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDb.Bson.NodaTime;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using SmartHotel.MixedReality.Api.Anchors;
using SmartHotel.MixedReality.Api.Auth;
using SmartHotel.MixedReality.Api.Data;
using SmartHotel.MixedReality.Api.Topology;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.NodaTime.AspNetCore;

namespace SmartHotel.MixedReality.Api
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
            NodaTimeSerializers.Register();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(o =>
            {
                o.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore;
                o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                o.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            });
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Smart Hotel Mixed Reality API", Version = "v1" });
                bool authEnabled =   Configuration.GetSection("AuthorizationSettings").GetValue<bool>("AuthEnabled");
                if (authEnabled)
                {
                    c.AddSecurityDefinition("ApiKeyAuth", new ApiKeyScheme() {Name = "X-API-KEY", In = "header", Description = "API Key Authentication"});
                    c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                    {
                        {"ApiKeyAuth", Enumerable.Empty<string>()},
                    });
                }
                c.ConfigureForNodaTime();
            });


            // Add functionality to inject IOptions<T>
            services.AddOptions();

            // Add our Config object so it can be injected
            services.Configure<AuthorizationSettings>(Configuration.GetSection("AuthorizationSettings"));
            services.Configure<DatabaseSettings>(Configuration.GetSection("DatabaseSettings"));
            services.Configure<DigitalTwinsSettings>(Configuration.GetSection("DigitalTwins"));
            services.Configure<SpatialServicesSettings>( Configuration.GetSection( "SpatialServices" ) );

            services.AddScoped(typeof(IDatabaseHandler<>), typeof(DatabaseHandler<>));
            services.AddScoped<IAnchorSetService, AnchorSetService>();
            services.AddScoped<AuthorizationFilterAttribute>();
            services.AddScoped<ITopologyClient, TopologyClient>();
            services.AddScoped<IDigitalTwinsClient, DigitalTwinsClient>();
            services.AddSingleton<IClock>(SystemClock.Instance);
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Hotel Mixed Reality API");
                c.RoutePrefix = string.Empty;
            });
            app.UseMvc();
        }
    }

    public class AuthorizationSettings
    {
        public string Apikey { get; set; }
        public bool AuthEnabled { get; set; } = true;
    }
}
