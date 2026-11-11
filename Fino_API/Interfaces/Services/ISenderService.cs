using FintechApp.Requests.Sender;
using FintechApp.Requests.SenderBeneficiary;
using FintechApp.Responses.Sender;
using FintechApp.Responses.SenderBeneficiary;
namespace FintechApp.Interfaces.Services
{
    public interface ISenderService
    {
        // The Sender / Beneficiary report — scoped to the caller, filtered, paged, with each
        // sender's owner and beneficiary count resolved.
        Task<(string failureMessage, SenderReportWrapper data)> SelectReport(
            SelectSenderReportRequest request, int callerUserID, int callerUserTypeID);

        Task<List<SenderResponse>> GetAll();
        Task<SenderResponse?> GetByID(int id);
        Task<List<SenderResponse>> GetByUserID(int userid);
        Task<string> Create(CreateSenderRequest request);
        Task<string> Update(UpdateSenderRequest request);
        Task<string> Delete(int id);
    }
}
