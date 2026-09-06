using System.Text.Json;
using Fino.JaiJuiApi.Configuration;
using Fino.JaiJuiApi.Interfaces;
using Fino.JaiJuiApi.Messages;
using Fino.JaiJuiApi.Requests;
using Fino.JaiJuiApi.Responses;
using Microsoft.Extensions.Configuration;

namespace Fino.JaiJuiApi.Services
{
    // Builds jaijuirecharge.in endpoint URLs, the "KeyData" credential header and the JSON request body,
    // then delegates the HTTP POST to IJaiJuiApiClient. Endpoint and body field names match the provider
    // specification exactly. Credentials come from the optional per-call override (the active DMT gateway
    // row) when supplied, otherwise from the "JaiJuiApi" configuration section — fully dynamic either way.
    public class JaiJuiApiService : IJaiJuiApiService
    {
        private readonly IJaiJuiApiClient _client;
        private readonly IConfiguration _config;

        public JaiJuiApiService(IJaiJuiApiClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
        }

        // Resolve effective options: start from the "JaiJuiApi" config section, then overlay any
        // non-blank fields from the per-call credentials override.
        private JaiJuiApiOptions Options(JaiJuiCredentials? creds)
        {
            var o = new JaiJuiApiOptions();
            _config.GetSection(JaiJuiApiOptions.SectionName).Bind(o);

            if (creds != null)
            {
                if (!string.IsNullOrWhiteSpace(creds.BaseUrl)) o.BaseUrl = creds.BaseUrl;
                if (!string.IsNullOrWhiteSpace(creds.Mobile)) o.Mobile = creds.Mobile;
                if (!string.IsNullOrWhiteSpace(creds.ApiToken)) o.ApiToken = creds.ApiToken;
                if (!string.IsNullOrWhiteSpace(creds.AuthCode)) o.AuthCode = creds.AuthCode;
            }

            if (string.IsNullOrWhiteSpace(o.BaseUrl)) o.BaseUrl = "https://api.jaijuirecharge.in/DMT/v1";
            o.BaseUrl = o.BaseUrl.TrimEnd('/');
            return o;
        }

        // KeyData header: {"Mobile":"RegisterMobileNo","APIToken":"Token"}
        private static string KeyData(JaiJuiApiOptions o) =>
            JsonSerializer.Serialize(new { Mobile = o.Mobile, APIToken = o.ApiToken });

        public Task<JaiJuiApiResponse> AccountVerify(AccountVerifyRequest request, JaiJuiCredentials? creds = null)
        {
            if (string.IsNullOrWhiteSpace(request.Mobile)) return Fail(JaiJui.Message.MobileRequired);
            if (string.IsNullOrWhiteSpace(request.AccountNo)) return Fail(JaiJui.Message.AccountNumberRequired);
            if (string.IsNullOrWhiteSpace(request.IFSC)) return Fail(JaiJui.Message.IFSCRequired);

            var o = Options(creds);
            var body = JsonSerializer.Serialize(new
            {
                AuthCode = o.AuthCode,
                Mobile = request.Mobile,
                SenderID = request.SenderID,
                BankName = request.BankName,
                AccountNo = request.AccountNo,
                IFSC = request.IFSC,
                PaymentType = string.IsNullOrWhiteSpace(request.PaymentType) ? "IMPS" : request.PaymentType,
                IsVerify = true,
                APIUserRequestID = request.APIUserRequestID,
                SenderName = request.SenderName
            });
            return _client.Post($"{o.BaseUrl}/MoneyTransfer", KeyData(o), body);
        }

        public Task<JaiJuiApiResponse> MoneyTransfer(MoneyTransferRequest request, JaiJuiCredentials? creds = null)
        {
            if (string.IsNullOrWhiteSpace(request.Mobile)) return Fail(JaiJui.Message.MobileRequired);
            if (string.IsNullOrWhiteSpace(request.AccountNo)) return Fail(JaiJui.Message.AccountNumberRequired);
            if (string.IsNullOrWhiteSpace(request.IFSC)) return Fail(JaiJui.Message.IFSCRequired);

            var o = Options(creds);
            var body = JsonSerializer.Serialize(new
            {
                AuthCode = o.AuthCode,
                Mobile = request.Mobile,
                BankName = request.BankName,
                AccountNo = request.AccountNo,
                IFSC = request.IFSC,
                BeneficiaryID = request.BeneficiaryID,
                SenderID = request.SenderID,
                IsVerify = false,
                APIUserRequestID = request.APIUserRequestID,
                PaymentType = string.IsNullOrWhiteSpace(request.PaymentType) ? "IMPS" : request.PaymentType,
                Amount = request.Amount,
                Name = request.Name
            });
            return _client.Post($"{o.BaseUrl}/MoneyTransfer", KeyData(o), body);
        }

        public Task<JaiJuiApiResponse> StatusCheck(StatusCheckRequest request, JaiJuiCredentials? creds = null)
        {
            if (string.IsNullOrWhiteSpace(request.ID)) return Fail(JaiJui.Message.RequestIDRequired);

            var o = Options(creds);
            var body = JsonSerializer.Serialize(new
            {
                AuthCode = o.AuthCode,
                ID = request.ID
            });
            return _client.Post($"{o.BaseUrl}/GetTransctionStatus", KeyData(o), body);
        }

        public Task<JaiJuiApiResponse> GetBalance(JaiJuiCredentials? creds = null)
        {
            var o = Options(creds);
            // Absolute BalanceUrl wins; otherwise compose {BaseUrl}/{BalancePath}.
            string balanceUrl = !string.IsNullOrWhiteSpace(o.BalanceUrl)
                ? o.BalanceUrl.Trim()
                : $"{o.BaseUrl}/{o.BalancePath.TrimStart('/')}";

            var body = JsonSerializer.Serialize(new { AuthCode = o.AuthCode });
            return _client.Post(balanceUrl, KeyData(o), body);
        }

        private static Task<JaiJuiApiResponse> Fail(string message) =>
            Task.FromResult(JaiJuiApiResponse.Fail(message));
    }
}
