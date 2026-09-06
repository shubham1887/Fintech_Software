using FintechApp.Requests.RangeWiseSequence;
using FintechApp.Responses.RangeWiseSequence;
namespace FintechApp.Interfaces.Services
{
    public interface IRangeWiseSequenceService
    {
        Task<List<RangeWiseSequenceResponse>> GetAll();
        Task<List<RangeWiseSequenceResponse>> GetByOperatorID(int operatorId);
        Task<RangeWiseSequenceResponse?> GetByID(int id);
        Task<string> Create(CreateRangeWiseSequenceRequest request);
        Task<string> Update(UpdateRangeWiseSequenceRequest request);
        Task<string> Delete(int id);
    }
}
