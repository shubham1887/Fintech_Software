using Fino.PlanApi.Configuration;
using Fino.PlanApi.Responses;

namespace Fino.PlanApi.Interfaces
{
    // Service layer: typed plan-data operations over planapi.in. Builds endpoint URLs (with
    // credentials resolved dynamically from configuration) and delegates the transport to IPlanApiClient.
    public interface IPlanApiService
    {
        Task<PlanApiResponse> MobilePlans(string operatorCode, string circle, PlanApiCredentials? creds = null);
        Task<PlanApiResponse> NewMobilePlans(string operatorCode, string circle, PlanApiCredentials? creds = null);
        Task<PlanApiResponse> RofferCheck(string operatorCode, string mobileNo, PlanApiCredentials? creds = null);
        Task<PlanApiResponse> OperatorFetch(string mobileNo, PlanApiCredentials? creds = null);
        Task<PlanApiResponse> DthPlans(string operatorCode, PlanApiCredentials? creds = null);
        Task<PlanApiResponse> DthInfoCheck(string opCode, string number, PlanApiCredentials? creds = null);
        Task<PlanApiResponse> DthBasicDetails(string opCode, string number, PlanApiCredentials? creds = null);
        Task<PlanApiResponse> DthInfoWithLastRecharge(string opCode, string number, PlanApiCredentials? creds = null);
        Task<PlanApiResponse> DthOperatorFetch(string dthNumber, PlanApiCredentials? creds = null);

        /// <summary>
        /// Electricity bill enquiry. <paramref name="optional1"/> carries the district/discom code
        /// where the discom needs one, <paramref name="optional2"/> the unit number printed on the
        /// bill; both are genuinely optional and are simply sent empty when not supplied. The
        /// provider's reply is parsed here because its shape is provider trivia (the "BILLDEATILS"
        /// misspelling, an ERROR of "0" on a failed fetch, amounts as "1,260.00").
        /// </summary>
        Task<ElectricityBillFetchResult> ElectricityBillFetch(string operatorCode, string billNumber, string optional1 = "", string optional2 = "", string optional3 = "", PlanApiCredentials? creds = null);

        /// <summary>District / discom codes to offer as Optional1 on a bill fetch.</summary>
        Task<ElectricityDistrictResult> ElectricityDistrictCodes(string circleId = "", PlanApiCredentials? creds = null);
    }
}
