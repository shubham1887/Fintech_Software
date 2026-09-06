using FintechApp.Requests.MicroATM;
using FintechApp.Responses.MicroATM;

namespace FintechApp.Interfaces.Services
{
    public interface IMicroATMService
    {
        Task<(string failureMessage, InitiateResponse? data)> Initiate(InitiateImproveRequest request, int userID, string ipAddress);
        Task<(string failureMessage, string responseJson)> TransactionStatusImprove(TransactionStatusRequest request, int userID);
    }
}
