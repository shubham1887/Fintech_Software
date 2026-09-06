using FintechApp.Helpers;
using FintechApp.Interfaces.Services;

namespace FintechApp.Services
{
    /// <summary>
    /// Drains the API-user callback queue and delivers each callback out of band.
    ///
    /// Runs a small fixed pool rather than one-at-a-time: a single unreachable receiver can hold
    /// a delivery for the full retry window, and one slow integrator must not delay everyone
    /// else's callbacks. The pool size is the backpressure — it bounds how many outbound HTTP
    /// calls this instance makes at once.
    /// </summary>
    public class ApiUserCallbackBackgroundService : BackgroundService
    {
        private readonly IApiUserCallbackQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ApiUserCallbackBackgroundService> _log;
        private readonly Logger _fileLogger = new();
        private readonly int _maxParallel;

        public ApiUserCallbackBackgroundService(
            IApiUserCallbackQueue queue,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<ApiUserCallbackBackgroundService> log)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _log = log;
            _maxParallel = Math.Clamp(configuration.GetValue("ApiUser:Callback:MaxParallel", 4), 1, 32);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _log.LogInformation("API-user callback sender started ({MaxParallel} parallel deliveries).", _maxParallel);

            using var throttler = new SemaphoreSlim(_maxParallel);
            var inFlight = new List<Task>();

            try
            {
                await foreach (int txnId in _queue.Reader.ReadAllAsync(stoppingToken))
                {
                    await throttler.WaitAsync(stoppingToken);

                    var task = Deliver(txnId, throttler);
                    inFlight.Add(task);
                    inFlight.RemoveAll(t => t.IsCompleted);
                }
            }
            catch (OperationCanceledException) { /* shutting down */ }

            // Give deliveries already handed to HttpClient a moment to finish so a restart does
            // not silently swallow a callback the integrator is waiting for.
            try { await Task.WhenAll(inFlight); } catch { /* each task already logs its own failure */ }

            _log.LogInformation("API-user callback sender stopped.");
        }

        private async Task Deliver(int transactionMasterID, SemaphoreSlim throttler)
        {
            try
            {
                // A fresh scope per delivery — the sender resolves a DbContext, which is neither
                // thread-safe nor valid beyond the request that created it.
                using var scope = _scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<IApiUserCallbackSender>();
                await sender.Send(transactionMasterID);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "API-user callback delivery failed for transaction {Id}.", transactionMasterID);
                _fileLogger.Write(ex);
            }
            finally
            {
                throttler.Release();
            }
        }
    }
}
