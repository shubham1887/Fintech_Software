using FintechApp.Interfaces.Providers;
using FintechApp.Requests.ProviderGateway;
using FintechApp.Responses.ProviderGateway;

namespace FintechApp.Interfaces.Services
{
    // Config-driven provider gateway. Everything an upstream integration needs — which provider
    // handles an operator, the endpoint URL/body template, HTTP method, headers, and how to read
    // the reply — is data (ProviderConfiguration / ProviderDetails / ProviderResponseMaster),
    // so new providers are onboarded from the admin panel without a code change.
    public interface IProviderGatewayService
    {
        // Dispatches a recharge through the configured route for the operator + service.
        Task<RechargeProviderResult> DispatchRecharge(RechargeDispatchContext context);

        // Runs the provider's BALANCE endpoint and maps the reply through the BALANCE rules.
        Task<(string failureMessage, ProviderBalanceResponse? data)> CheckBalance(int providerID);

        // Same enquiry against every active provider — what the dashboard's refresh button uses to
        // replace the stored (and therefore stale) provider stock with live figures.
        Task<(string failureMessage, List<ProviderBalanceResponse> data)> CheckAllBalances();

        // Runs the provider's BILLFETCH endpoint for an operator + consumer number and maps the
        // reply through the BILLFETCH rules (customer name, bill amount, due date …).
        Task<(string failureMessage, BillFetchResponse? data)> FetchBill(BillFetchContext context);

        // Runs the provider's STATUS endpoint for one transaction and maps the reply through
        // the STATUS rules. transactionDate feeds [Date] (e.g. Payin's DateInteger=yyyyMMdd).
        Task<ProviderStatusResult> CheckTransactionStatus(ProviderStatusContext context, DateTime transactionDate);

        // Dry-runs a sample reply through the provider's response rules (no HTTP call).
        Task<(string failureMessage, ResponseRuleTestResponse? data)> TestResponse(TestProviderResponseRequest request);

        // Template tokens supported by the engine — the UI renders these as helper chips.
        List<PlaceholderInfo> GetPlaceholders();
    }
}
