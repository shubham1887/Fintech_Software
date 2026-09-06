using System.Threading.Channels;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IApiUserCallbackQueue"/>
    /// <remarks>
    /// Bounded on purpose: an unbounded queue turns a callback backlog (every integrator's
    /// receiver down at once) into unbounded memory growth. At capacity the enqueue is dropped
    /// and logged rather than blocking a transaction — the integrator can still recover the
    /// result from /api/v1/status, which is exactly what that endpoint is for.
    /// </remarks>
    public class ApiUserCallbackQueue : IApiUserCallbackQueue
    {
        private readonly Channel<int> _channel;
        private readonly Logger _logger = new();

        public ApiUserCallbackQueue(IConfiguration configuration)
        {
            int capacity = Math.Clamp(configuration.GetValue("ApiUser:Callback:QueueCapacity", 5000), 100, 100000);
            _channel = Channel.CreateBounded<int>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
                SingleWriter = false,
            });
        }

        public ChannelReader<int> Reader => _channel.Reader;

        public bool Enqueue(int transactionMasterID)
        {
            if (transactionMasterID <= 0) return false;

            bool queued = _channel.Writer.TryWrite(transactionMasterID);
            if (!queued)
                _logger.Write(new Exception(
                    $"API-user callback queue is full; callback for transaction {transactionMasterID} was dropped. " +
                    "The integrator can still read the result from /api/v1/status."));
            return queued;
        }
    }
}
