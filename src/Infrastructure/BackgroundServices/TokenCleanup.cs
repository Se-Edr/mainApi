using Infrastructure.DataContext;
using Infrastructure.GeneralSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.BackgroundServices
{
    internal class TokenCleanup : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CommonSettings _commonSettings;

        public TokenCleanup(IServiceProvider serviceProvider, IOptions<CommonSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _commonSettings=settings.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanTokenTables();
                await Task.Delay(TimeSpan.FromMinutes(_commonSettings.BackGroundCleanTimeSpan), stoppingToken); //FromHours(1)
            }
            throw new NotImplementedException();
        }

        private async Task CleanTokenTables()
        {
            using (var scope= _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<GarageContext>();

                
                var expiredRefreshTokens = context.TokensTable.Where(token => token.Expires < DateOnly.FromDateTime(DateTime.UtcNow));

                var expiredUpdateUserTokens = context.EditUserTokenTable.Where(token => token.ExpiryDate < DateTime.UtcNow);

                if(expiredRefreshTokens.Any()|| expiredUpdateUserTokens.Any())
                {
                    context.TokensTable.RemoveRange(expiredRefreshTokens);
                    context.EditUserTokenTable.RemoveRange(expiredUpdateUserTokens);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
