using FintechApp.Requests.PureDMTTransferBeneDetail;
using FintechApp.Responses.PureDMTTransferBeneDetail;
namespace FintechApp.Interfaces.Services
{
    public interface IPureDMTTransferBeneDetailService
    {
        Task<List<PureDMTTransferBeneDetailResponse>> GetAll();
        Task<PureDMTTransferBeneDetailResponse?> GetByID(int id);

        Task<string> Create(CreatePureDMTTransferBeneDetailRequest request);
        Task<string> Update(UpdatePureDMTTransferBeneDetailRequest request);
        Task<string> Delete(int id);
    }
}
