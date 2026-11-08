using Yavsc.Server.Helpers;

namespace Yavsc.Web.Services;

public sealed class PaymentResiduePurgeWorker(IServiceScopeFactory scopes,
    ILogger<PaymentResiduePurgeWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeSpan.FromMinutes(1);
            try
            {
                using var scope = scopes.CreateScope();
                var residues = scope.ServiceProvider.GetRequiredService<IPaymentResidueService>();
                await residues.PurgeAsync(stoppingToken);
                var clock = scope.ServiceProvider.GetRequiredService<TimeProvider>();
                var now = clock.GetUtcNow();
                var expiry = await residues.GetNextExpiryAsync(stoppingToken);
                if (expiry is not null)
                {
                    var until = expiry.Value - now;
                    if (until < delay) delay = until > TimeSpan.Zero ? until : TimeSpan.Zero;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Automatic payment residue purge failed; retrying in one minute");
            }
            await Task.Delay(delay, stoppingToken);
        }
    }
}
