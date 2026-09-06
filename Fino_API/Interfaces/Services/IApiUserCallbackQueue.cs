using System.Threading.Channels;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Hand-off point between a request thread and the background callback sender.
    ///
    /// Why it exists: callback delivery retries a slow or unreachable receiver, which can take
    /// tens of seconds. Doing that inline would add that time to the integrator's own
    /// /api/v1/recharge response — their transaction would appear to hang because THEIR server
    /// was slow. Enqueuing takes microseconds and the delivery happens out of band.
    /// </summary>
    public interface IApiUserCallbackQueue
    {
        /// <summary>Queue a settled transaction for callback delivery. Never blocks.</summary>
        bool Enqueue(int transactionMasterID);

        /// <summary>Consumed by the background sender.</summary>
        ChannelReader<int> Reader { get; }
    }
}
