using Application.ConfigCalsses;
using Application.DataServices;
using Application.Middlewares;
using Application.Services;
using Application.Validators;
using Azure.Storage.Blobs;
using FluentValidation;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Infrastructure.Repos.UOW;

namespace Application.Extensions;

public static class AppExtensions
{
    public static void AddAppExts(this IServiceCollection services,IConfiguration config)
    {
        //register IOptions     
        services.Configure<JWTSettings>(config.GetSection("Jwt"));
        services.Configure<ApiSettings>(config.GetSection("ApiConfig"));
        services.Configure<EmailSenderSettings>(config.GetSection("EmailSender"));


        services.AddSingleton(b=> new BlobServiceClient(config.GetConnectionString("BlobConnectionString")));

        var appAssembly = typeof(AppExtensions).Assembly;
        services.AddValidatorsFromAssembly((appAssembly),includeInternalTypes:true); //better approach

        //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("Jwt:Key").Value));
        //TokenValidationParameters validationparametres = new TokenValidationParameters()
        //{
        //    ValidateIssuer = true,
        //    ValidateAudience = true,
        //    ValidateLifetime = true,
        //    ValidateIssuerSigningKey = true,
        //    ValidIssuer = config.GetSection("Jwt:Issuer").Value,
        //    ValidAudience = config.GetSection("Jwt:Audience").Value,
        //    IssuerSigningKey = key,//new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("Jwt:Key").Value)),
        //    ClockSkew = TimeSpan.Zero
        //};

       

        //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt => opt.TokenValidationParameters = validationparametres);

        //services.AddAuthorization();





        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = "http://192.168.1.175:1330/realms/finance_app_realm";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = false,
                    //RoleClaimType= "resource_access.finances_avantime_app_client.roles",
                    ValidIssuer = "http://192.168.1.175:1330/realms/finance_app_realm"
                };


                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        var identity = ctx.Principal!.Identity as ClaimsIdentity;
                        var resourceAccessClaim = identity?.FindFirst("resource_access");
                        if (resourceAccessClaim != null)
                        {
                            // распарсить только один раз
                            using var doc = JsonDocument.Parse(resourceAccessClaim.Value);
                            if (doc.RootElement.TryGetProperty("finances_avantime_app_client", out var client))
                            {
                                if (client.TryGetProperty("roles", out var roles))
                                {
                                    foreach (var role in roles.EnumerateArray())
                                    {
                                        identity!.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                                    }
                                }
                            }
                        }
                        return Task.CompletedTask;
                    }
                };


            });

        services.AddAuthorization()
            .AddKeycloakAuthorization(config)
            ;




        //services.AddScoped<IValidator<CarDTO>, CarValidator>();

        services.AddScoped<JWTTokenService>();
        services.AddScoped<EmailSenderSettings>();
        services.AddScoped<EmailSender>();
        services.AddScoped<QRCodeService>();
        services.AddScoped<BlobService>();
        services.AddScoped<MinioService>();
        services.AddScoped<PhotoResizerService>();
        services.AddScoped<CryptingService>();
        services.AddScoped<PasswordService>();

        services.AddScoped<AuthService>();
        services.AddScoped<GarageService>();
        services.AddScoped<RepairService>();
        services.AddScoped<UserCarService>();
        services.AddScoped<AuthWithKeycIoakService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
    

    public static IApplicationBuilder AddTokenMiddlewre(this IApplicationBuilder builder)
    {
        string[] closedPaths = new[] { "/api/Cars", "/api/Repairs", "/api/UserCars", "/api/Auth/UpdateTokens",  };
        return 
        builder.UseWhen
            (
            context => closedPaths.Any(path=>context.Request.Path.StartsWithSegments(path)),
            appbuilder => appbuilder.UseMiddleware<TokenComparerMiddleware>()
            );
    }

}
