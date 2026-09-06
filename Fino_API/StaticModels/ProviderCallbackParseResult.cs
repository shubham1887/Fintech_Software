using FintechApp.Requests.TransactionProcessing;

namespace FintechApp.StaticModels
{
    /// <summary>
    /// What a provider's push means, once its own format has been translated.
    ///
    /// <see cref="Verified"/> is the gate the router settles on: a push that does not verify is
    /// logged and dropped, never applied to a wallet. A parser that has no way to prove
    /// authenticity (a provider that signs nothing) says so in <see cref="Message"/> and leans on
    /// the IP allowlist the router enforces before it is even called.
    /// </summary>
    public class ProviderCallbackParseResult
    {
        public bool Verified { get; set; }
        public string Message { get; set; } = string.Empty;
        public RechargeCallbackRequest Callback { get; set; } = new();

        public static ProviderCallbackParseResult Reject(string message) => new() { Message = message };

        public static ProviderCallbackParseResult Accept(RechargeCallbackRequest callback, string message = "Verified.") =>
            new() { Verified = true, Message = message, Callback = callback };
    }
}
