using Fino.PlanApi.Configuration;
using Fino.PlanApi.Interfaces;
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

        private PlanApiOptions Options()
        {
            var o = new PlanApiOptions();
            _config.GetSection(PlanApiOptions.SectionName).Bind(o);
            if (string.IsNullOrWhiteSpace(o.BaseUrl)) o.BaseUrl = "https://planapi.in/api/Mobile";
            o.BaseUrl = o.BaseUrl.TrimEnd('/');
            return o;
        }

        private static string E(string v) => Uri.EscapeDataString(v ?? string.Empty);

        public Task<PlanApiResponse> MobilePlans(string operatorCode, string circle)
        {
            var o = Options();
            return _client.Get($"{o.BaseUrl}/MobileRechargePlan?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&operatorcode={E(operatorCode)}&cricle={E(circle)}");
        }

        public Task<PlanApiResponse> NewMobilePlans(string operatorCode, string circle)
        {
            var o = Options();
            return _client.Get($"{o.BaseUrl}/NewMobilePlans?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&operatorcode={E(operatorCode)}&cricle={E(circle)}");
        }

        public Task<PlanApiResponse> RofferCheck(string operatorCode, string mobileNo)
        {
            var o = Options();
            return _client.Get($"{o.BaseUrl}/RofferCheck?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&operator_code={E(operatorCode)}&mobile_no={E(mobileNo)}");
        }

        public Task<PlanApiResponse> OperatorFetch(string mobileNo)
        {
            var o = Options();
            return _client.Get($"{o.BaseUrl}/OperatorFetchNew?ApiUserID={E(o.MemberId)}&ApiPassword={E(o.Password)}&Mobileno={E(mobileNo)}");
        }

        public Task<PlanApiResponse> DthPlans(string operatorCode)
        {
            var o = Options();
            return _client.Get($"{o.BaseUrl}/DthPlans?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&operatorcode={E(operatorCode)}");
        }

        public Task<PlanApiResponse> DthInfoCheck(string opCode, string number)
        {
            var o = Options();
            return _client.Get($"{o.BaseUrl}/DTHINFOCheck?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&mobile_no={E(number)}&Opcode={E(opCode)}");
        }

        public Task<PlanApiResponse> DthBasicDetails(string opCode, string number)
        {
            var o = Options();
            return _client.Get($"{o.BaseUrl}/DTHBasicDetails?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&mobile_no={E(number)}&Opcode={E(opCode)}");
        }

        public Task<PlanApiResponse> DthInfoWithLastRecharge(string opCode, string number)
        {
            var o = Options();
            return _client.Get($"{o.BaseUrl}/DthInfoWithLastRechargeDate?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&mobile_no={E(number)}&Opcode={E(opCode)}");
        }

        public Task<PlanApiResponse> DthOperatorFetch(string dthNumber)
        {
            var o = Options();
            return _client.Get($"{o.BaseUrl}/DthOperatorFetch?apimember_id={E(o.MemberId)}&api_password={E(o.Password)}&dth_number={E(dthNumber)}");
        }
    }
}
