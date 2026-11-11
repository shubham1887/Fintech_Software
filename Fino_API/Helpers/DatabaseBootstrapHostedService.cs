namespace FintechApp.Helpers
{
    /// <summary>
    /// Where the application is in its first-run database setup. Read by /health, which is what the
    /// installer and the container healthcheck wait on.
    /// </summary>
    public enum BootstrapState
    {
        Pending,
        Running,
        Ready,
        Failed
    }

    /// <summary>Shared, thread-safe readiness flag. Singleton.</summary>
    public sealed class BootstrapStatus
    {
        private volatile string? _error;
        private int _state = (int)BootstrapState.Pending;

        public BootstrapState State => (BootstrapState)Volatile.Read(ref _state);
        public string? Error => _error;
        public bool IsReady => State == BootstrapState.Ready;

        public void Set(BootstrapState state, string? error = null)
        {
            _error = error;
            Volatile.Write(ref _state, (int)state);
        }
    }

    /// <summary>
    /// Runs the database setup AFTER the host has started, rather than before it.
    ///
    /// The reason is Windows: a service has to report "started" to the Service Control Manager
    /// within about 30 seconds, and building this schema from empty takes two to three minutes. Do
    /// the work before <c>app.Run()</c> and the SCM kills a perfectly healthy installation halfway
    /// through creating its tables. So the host starts immediately and this runs behind it, while
    /// /health reports "starting" and the middleware holds API calls off with a 503 until the
    /// database is actually there.
    /// </summary>
    public sealed class DatabaseBootstrapHostedService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseBootstrapHostedService> _logger;
        private readonly IHostEnvironment _environment;
        private readonly BootstrapStatus _status;

        public DatabaseBootstrapHostedService(
            IConfiguration config,
            ILogger<DatabaseBootstrapHostedService> logger,
            IHostEnvironment environment,
            BootstrapStatus status)
        {
            _config = config;
            _logger = logger;
            _environment = environment;
            _status = status;
        }

        // Retries exist for one specific case: the machine has just booted and SQL Server is still
        // starting, so the first connection is refused. Without them the API would sit at 503 until
        // somebody restarted the service, for a database that came up thirty seconds later.
        private const int MaxAttempts = 4;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(20);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    _status.Set(BootstrapState.Running);
                    await DatabaseBootstrapper.RunAsync(_config, _logger, _environment.ContentRootPath);
                    _status.Set(BootstrapState.Ready);
                    return;
                }
                catch (Exception ex) when (attempt < MaxAttempts && !stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(ex,
                        "Database setup attempt {Attempt} of {Max} failed; retrying in {Seconds}s.",
                        attempt, MaxAttempts, RetryDelay.TotalSeconds);
                    try { await Task.Delay(RetryDelay, stoppingToken); } catch (OperationCanceledException) { return; }
                }
                catch (Exception ex)
                {
                    // Deliberately not fatal. A service that exits here restarts, fails the same way
                    // and leaves the installer with nothing to read; staying up means /health can say
                    // what went wrong, which is what somebody fixing the install actually needs.
                    _status.Set(BootstrapState.Failed, ex.Message);
                    _logger.LogCritical(ex,
                        "Database setup failed after {Attempts} attempts. The API will refuse requests until this is fixed and the service is restarted.",
                        MaxAttempts);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Refuses API calls while the database is still being built, so a request that arrives during
    /// the first three minutes gets an honest 503 instead of a confusing SQL error.
    /// </summary>
    public sealed class BootstrapGateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BootstrapStatus _status;

        public BootstrapGateMiddleware(RequestDelegate next, BootstrapStatus status)
        {
            _next = next;
            _status = status;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // /health has to stay reachable — it is how anyone finds out what this state is.
            var path = context.Request.Path.Value ?? string.Empty;
            if (_status.IsReady || !path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers.RetryAfter = "30";
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = 503,
                status = "STARTING",
                message = _status.State == BootstrapState.Failed
                    ? "The server could not finish setting up its database. Contact support."
                    : "The server is still setting up its database. This takes a few minutes on a new installation.",
                data = ""
            });
        }
    }
}
