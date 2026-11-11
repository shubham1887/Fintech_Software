using FintechApp.Entities;
using FintechApp.Responses.ProviderGateway;

namespace FintechApp.Interfaces.Providers
{
    /// <summary>
    /// A provider reached through a COMPILED LIBRARY instead of endpoint templates.
    ///
    /// Most providers need no code at all: ProviderDetails holds a URL/body template, response
    /// rules read the reply, and the gateway does the rest. A few cannot be expressed that way —
    /// Pay1 hashes its API key into every request and never transmits it; JaiJui ships its own
    /// client. Those arrive as a sibling project (Fino.Pay1Api, Fino.JaiJuiApi) and need an
    /// adapter that turns the gateway's neutral context into that library's call and its reply
    /// back into the gateway's neutral result.
    ///
    /// This interface IS that adapter, and it is the only place a new coded provider has to
    /// touch. Before it existed, "Pay1" was named in five separate branches inside
    /// ProviderGatewayService — dispatch, bill fetch, status, balance — plus the service-family
    /// map, and adding a provider meant finding all of them. Missing one did not fail loudly: the
    /// gateway quietly fell back to looking for an endpoint template that does not exist, so (for
    /// example) status checks would silently stop working while payments still went out.
    ///
    /// Every operation is OPTIONAL. A provider implements only what its library offers and leaves
    /// the rest alone; returning null means "not mine", and the gateway carries on with the
    /// template path exactly as it would for any other provider.
    ///
    /// Registration order in Program.cs is the match order, the same as IProviderCallbackParser.
    /// </summary>
    public interface ICodedProvider
    {
        /// <summary>True when this adapter is the integration behind that "API Providers" row.</summary>
        bool CanHandle(ProviderMaster provider);

        /// <summary>
        /// The rail this provider serves — a <see cref="StaticModels.ServiceKinds"/> value such as
        /// BBPS or DMT. A coded provider declares no endpoints, so this is what tells Provider
        /// Routing which services it may be switched onto; without it the provider is configured,
        /// credentialed, active, and visible in no routing grid anywhere.
        /// </summary>
        string Family { get; }

        /// <summary>Send a transaction (recharge / bill payment). Null = not handled here.</summary>
        Task<RechargeProviderResult?> Dispatch(
            ProviderMaster provider, ProviderConfiguration route, RechargeDispatchContext context)
            => Task.FromResult<RechargeProviderResult?>(null);

        /// <summary>Read a biller's outstanding bill before payment. Null = not handled here.</summary>
        Task<CodedBillFetchResult?> FetchBill(
            ProviderMaster provider, ProviderConfiguration route, BillFetchContext context)
            => Task.FromResult<CodedBillFetchResult?>(null);

        /// <summary>Ask the provider what became of a transaction. Null = not handled here.</summary>
        Task<ProviderStatusResult?> CheckStatus(ProviderMaster provider, ProviderStatusContext context)
            => Task.FromResult<ProviderStatusResult?>(null);

        /// <summary>Fetch the provider's wallet balance. Null = not handled here.</summary>
        Task<CodedBalanceResult?> CheckBalance(ProviderMaster provider)
            => Task.FromResult<CodedBalanceResult?>(null);
    }

    /// <summary>
    /// A bill-fetch outcome. <see cref="Data"/> null means the bill could not be read, and
    /// <see cref="FailureMessage"/> says why — no money is involved, so the caller moves on to
    /// the next provider in the sequence.
    /// </summary>
    public sealed record CodedBillFetchResult(string FailureMessage, BillFetchResponse? Data);

    /// <summary>A balance-enquiry outcome, shaped like the template path's so callers are blind
    /// to which kind of provider answered.</summary>
    public sealed record CodedBalanceResult(string FailureMessage, ProviderBalanceResponse? Data);
}
