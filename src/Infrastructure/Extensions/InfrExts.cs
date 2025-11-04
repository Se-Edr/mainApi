using Domain.AppSettings;
using Domain.Repos;
using Infrastructure.BackgroundServices;
using Infrastructure.DataContext;
using Infrastructure.GeneralSettings;
using Infrastructure.PaginationServices;
using Infrastructure.Repos;
using Infrastructure.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class InfrExts
{
    public static void AddIfrastruct(this IServiceCollection services ,IConfiguration config)
    {
        string connstr = config.GetConnectionString("ConnStringToNew")!;

        //services.Configure<CommonSettings>(config.GetSection("CommonSettings"));
        services.Configure<CommonSettings>(opt =>config.GetSection("CommonSettings").Bind(opt));

        //services.Configure<Encryption>(config.GetSection("Encryption"));
        services.Configure<Encryption>(opt=>config.GetSection("Encryption").Bind(opt));

        services.Configure<MinioSettings>(opt => config.GetSection("MinioSettings").Bind(opt));




        services.AddDbContext<GarageContext>(options => options.UseSqlServer(connstr));
        services.AddHostedService<TokenCleanup>();


       

        services.AddScoped<IGarageRepo,GargeRepo>();
        services.AddScoped<IRepairRepo,RepairRepo>();
        services.AddScoped<IUserRepo,UserRepo>();
        services.AddScoped<IUserCarsRepo,UserCarsRepo>();
        services.AddScoped<RoleManager>();
        services.AddScoped<PagService>();
    

        services.AddScoped<IRoleSeeder, RoleSeeder>();

        services.AddHealthChecks().AddSqlServer(connstr);
    }
}
