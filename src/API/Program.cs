using Infrastructure.Extensions;
using Application.Extensions;
using HealthChecks.UI.Client;
using Avantime.API.Exception;

namespace AvantimeApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<ExceptionMiddleware>();

            builder.Services.AddIfrastruct(builder.Configuration);
            builder.Services.AddAppExts(builder.Configuration);

            builder.Services.AddHealthChecks();


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp3000",
                    builder =>
                    {
                        builder
                        .WithOrigins("http://localhost:5173")
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
                        // FIX COOKIE IN AUTHCONTROLLER FOR HTTPS  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    });
            });


            //configureLogging();
            //builder.Host.UseSerilog();

            var app = builder.Build();
            app.UseMiddleware<ExceptionMiddleware>();
            //app.UseExceptionHandler(o => { });


            app.UseCors("AllowReactApp3000");
            

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            app.UseAuthentication();
            //app.AddTokenMiddlewre();
            app.UseAuthorization();


            app.MapControllers();

            app.MapHealthChecks("health",new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter=UIResponseWriter.WriteHealthCheckUIResponse
            });
           

            app.Run();
        }

        //static void configureLogging()
        //{
        //    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        //    var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json",optional:false,reloadOnChange:true)
        //        .AddJsonFile($"appsettings.{environment}.json",true).Build();

        //    string url = configuration.GetSection("ElasticConfig:Uri").Value;

        //    Serilog.Log.Logger = new LoggerConfiguration()
        //        .Enrich.FromLogContext()
        //        .Enrich.WithExceptionDetails()
        //        .WriteTo.Debug()
        //        .WriteTo.Console()
        //        .WriteTo.Elasticsearch(new[] { new Uri(url) },
        //              options =>
        //              {
        //                  options.TextFormatting = new EcsTextFormatterConfiguration();
        //                  options.BootstrapMethod = BootstrapMethod.Failure;
        //                  options.ConfigureChannel = channelOptions =>
        //                  {
        //                      channelOptions.BufferOptions = new BufferOptions();
        //                  };
        //              })
        //        .Enrich.WithProperty("Environment",environment)
        //        .ReadFrom.Configuration(configuration)
        //        .CreateLogger();
        //}
       
    }
}
