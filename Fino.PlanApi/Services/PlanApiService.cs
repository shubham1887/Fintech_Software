using Fino.PlanApi.Configuration;
using Fino.PlanApi.Interfaces;
using Fino.PlanApi.Messages;
using Fino.PlanApi.Responses;
using Microsoft.Extensions.Configuration;

namespace Fino.PlanApi.Services
{
    // Builds planapi.in endpoint URLs (with credentials resolved at call time from the "PlanApi"
    // configuration section — fully dynamic) and delegates the HTTP call to IPlanApiClient.
    // Endpoint and query parameter names match the provider specification exactly.
    public class PlanApiService : IPlanApiService
    {
        private readonly IPlanApiClient _client;
        private readonly IConfiguration _config;

        public PlanApiService(IPlanApiClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
        }

        /// <summary>
        /// Effective settings for one call: the "PlanApi" configuration section, with any non-blank
        /// field of <paramref name="creds"/> laid over it. A host that keeps the account on its own
        /// provider row passes credentials per call; one that does not passes null and nothing changes.
        /// </summary>
        private PlanApiOptions Options(PlanApiCredentials? creds = null)
        {
            var o = new PlanApiOptions();
            _config.GetSection(PlanApiOptions.SectionName).Bind(o);

            if (creds != null)
            {
                if (!string.IsNullOrWhiteSpace(creds.BaseUrl)) o.BaseUrl = creds.BaseUrl.Trim();
                if (!string.IsNullOrWhiteSpace(creds.MemberId)) o.MemberId = creds.MemberId.Trim();
                if (!string.IsNullOrWhiteSpace(creds.Password)) o.Password = creds.Password.Trim();
            }

            if (string.IsNullOrWhiteSpace(o.BaseUrl)) o.BaseUrl = "https://planapi.in/api/Mobile";
            o.BaseUrl = o.BaseUrl.TrimEnd('/');
            return o;
        }

        private static string E(string v) => Uri.EscapeDataString(v ?? string.Empty);

        public Task<PlanApiResponse> MobilePlans(string operatorCode, string circle, PlanApiCredentials? creds = null)
        {
            var o = Options(creds);
            return _client.Get($"{o.BaseUrl}/MobileRechargePlan?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&operatorcode={E(operatorCode)}&cricle={E(circle)}");
        }

        public Task<PlanApiResponse> NewMobilePlans(string operatorCode, string circle, PlanApiCredentials? creds = null)
        {
            var o = Options(creds);
            return _client.Get($"{o.BaseUrl}/NewMobilePlans?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&operatorcode={E(operatorCode)}&cricle={E(circle)}");
        }

        public Task<PlanApiResponse> RofferCheck(string operatorCode, string mobileNo, PlanApiCredentials? creds = null)
        {
            var o = Options(creds);
            return _client.Get($"{o.BaseUrl}/RofferCheck?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&operator_code={E(operatorCode)}&mobile_no={E(mobileNo)}");
        }

        public Task<PlanApiResponse> OperatorFetch(string mobileNo, PlanApiCredentials? creds = null)
        {
            var o = Options(creds);
            return _client.Get($"{o.BaseUrl}/OperatorFetchNew?ApiUserID={E(o.MemberId)}&ApiPassword={E(o.Password)}&Mobileno={E(mobileNo)}");
        }

        public Task<PlanApiResponse> DthPlans(string operatorCode, PlanApiCredentials? creds = null)
        {
            var o = Options(creds);
            return _client.Get($"{o.BaseUrl}/DthPlans?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&operatorcode={E(operatorCode)}");
        }

        public Task<PlanApiResponse> DthInfoCheck(string opCode, string number, PlanApiCredentials? creds = null)
        {
            var o = Options(creds);
            return _client.Get($"{o.BaseUrl}/DTHINFOCheck?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&mobile_no={E(number)}&Opcode={E(opCode)}");
        }

        public Task<PlanApiResponse> DthBasicDetails(string opCode, string number, PlanApiCredentials? creds = null)
        {
            var o = Options(creds);
            return _client.Get($"{o.BaseUrl}/DTHBasicDetails?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&mobile_no={E(number)}&Opcode={E(opCode)}");
        }

        public Task<PlanApiResponse> DthInfoWithLastRecharge(string opCode, string number, PlanApiCredentials? creds = null)
        {
            var o = Options(creds);
            return _client.Get($"{o.BaseUrl}/DthInfoWithLastRechargeDate?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&mobile_no={E(number)}&Opcode={E(opCode)}");
        }

        public Task<PlanApiResponse> DthOperatorFetch(string dthNumber, PlanApiCredentials? creds = null)
        {
            var o = Options(creds);
            return _client.Get($"{o.BaseUrl}/DthOperatorFetch?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&dth_number={E(dthNumber)}");
        }

        // --- Electricity ----------------------------------------------------------------------

        public async Task<ElectricityBillFetchResult> ElectricityBillFetch(string operatorCode, string billNumber, string optional1 = "", string optional2 = "", string optional3 = "", PlanApiCredentials? creds = null)
        {
            var o = Options(creds);
            // Optional1..3 are always present in the query string, empty when unused: that is the
            // shape the provider's own working sample uses, and some discoms reject a missing key.
            string url = $"{o.BaseUrl}/ElectricityBillFetch?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}" +
                         $"&bill_number={E(billNumber)}&operator_code={E(operatorCode)}" +
                         $"&Optional1={E(optional1)}&Optional2={E(optional2)}&Optional3={E(optional3)}";

            var response = await _client.Get(url);
            if (!response.Success) return ElectricityBillFetchResult.Fail(response.Message);
            return ElectricityBillFetchResult.Parse(response.Data, Plan.Message.BillFetchFailed, billNumber);
        }

        public async Task<ElectricityDistrictResult> ElectricityDistrictCodes(string circleId = "", PlanApiCredentials? creds = null)
        {
            var o = Options(creds);
            // CircleId is forwarded as the provider documents it. It currently publishes districts
            // for one circle only and ignores the filter, so the full list is returned unchanged
            // rather than filtered here — filtering client-side would empty the dropdown instead.
            string url = $"{o.BaseUrl}/ElectricityDistrictCode";
            if (!string.IsNullOrWhiteSpace(circleId)) url += $"?CircleId={E(circleId)}";

            var response = await _client.Get(url);
            if (!response.Success) return ElectricityDistrictResult.Fail(response.Message);
            return ElectricityDistrictResult.Parse(response.Data, Plan.Message.DistrictCodeUnavailable);
        }
    }
}
