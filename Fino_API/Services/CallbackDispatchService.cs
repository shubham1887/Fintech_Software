using FintechApp.Interfaces.Services;

namespace FintechApp.Services
{
    /// <inheritdoc cref="ICallbackDispatchService"/>
    /// <remarks>
    /// Deliberately trivial: it hands the transaction id to <see cref="IApiUserCallbackQueue"/>
    /// and returns. <see cref="ApiUserCallbackSender"/>, driven by
    /// <see cref="ApiUserCallbackBackgroundService"/>, does the delivery.
    ///
    /// This is what keeps callback delivery off the request path. Every settlement path calls
    /// this from inside an HTTP request — the synchronous /api/v1/recharge reply, a status
    /// check, the provider's push callback — and the receiver on the other end may be slow,
    /// unreachable, or need all three retry attempts. Doing that inline would charge the
    /// integrator's own response time for their server being down.
    /// </remarks>
    public class CallbackDispatchService : ICallbackDispatchService
    {
        private readonly IApiUserCallbackQueue _queue;

        public CallbackDispatchService(IApiUserCallbackQueue queue) => _queue = queue;

        public Task DispatchForTransaction(int transactionMasterID)
        {
            _queue.Enqueue(transactionMasterID);
            return Task.CompletedTask;
        }
    }
}
