using FintechApp.Requests.AgentDetails;
using FintechApp.Responses.AgentDetails;

namespace FintechApp.Interfaces.Services
{
    public interface IAgentDetailsService
    {
        Task<(string failureMessage, int successfulUpdates, int totalRecords)> Upload(AgentUploadRequest request);
        Task<(string failureMessage, AgentUserWrapper? data)> SelectByPinCode(SelectPincodeRequest request);
    }
}
