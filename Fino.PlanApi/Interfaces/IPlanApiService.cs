using Fino.PlanApi.Responses;

namespace Fino.PlanApi.Interfaces
{
    // Service layer: typed plan-data operations over planapi.in. Builds endpoint URLs (with
    // credentials resolved dynamically from configuration) and delegates the transport to IPlanApiClient.
    public interface IPlanApiService
    {
        Task<PlanApiResponse> MobilePlans(string operatorCode, string circle);
        Task<PlanApiResponse> NewMobilePlans(string operatorCode, string circle);
        Task<PlanApiResponse> RofferCheck(string operatorCode, string mobileNo);
        Task<PlanApiResponse> OperatorFetch(string mobileNo);
        Task<PlanApiResponse> DthPlans(string operatorCode);
        Task<PlanApiResponse> DthInfoCheck(string opCode, string number);
        Task<PlanApiResponse> DthBasicDetails(string opCode, string number);
        Task<PlanApiResponse> DthInfoWithLastRecharge(string opCode, string number);
        Task<PlanApiResponse> DthOperatorFetch(string dthNumber);
    }
}
