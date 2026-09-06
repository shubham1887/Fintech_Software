using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.LapuMaster;
using FintechApp.Responses.LapuMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class LapuMasterService : ILapuMasterService
    {
        private readonly ILapuMasterRepository _repository;
        public LapuMasterService(ILapuMasterRepository repository) { _repository = repository; }

        private static LapuMasterResponse MapToResponse(LapuMaster e) => new()
        {
            LapuMasterID = e.LapuMasterID,
            LapuPartyMasterID = e.LapuPartyMasterID,
            OperatorID = e.OperatorID,
            LapuNo = e.LapuNo,
            ProivderLapuID = e.ProivderLapuID,
            ClosingBalance = e.ClosingBalance,
        };

        public async Task<List<LapuMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<LapuMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateLapuMasterRequest request)
        {
            var entity = new LapuMaster
            {
                LapuPartyMasterID = request.LapuPartyMasterID,
                OperatorID = request.OperatorID,
                LapuNo = request.LapuNo,
                ProivderLapuID = request.ProivderLapuID,
                ClosingBalance = request.ClosingBalance,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateLapuMasterRequest request)
        {
            var entity = await _repository.GetByID(request.LapuMasterID);
            if (entity == null) return Messages.LapuMasterMessages.NotFound;
            entity.LapuPartyMasterID = request.LapuPartyMasterID;
            entity.OperatorID = request.OperatorID;
            entity.LapuNo = request.LapuNo;
            entity.ProivderLapuID = request.ProivderLapuID;
            entity.ClosingBalance = request.ClosingBalance;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.LapuMasterMessages.NotFound;
        }
    }
}
