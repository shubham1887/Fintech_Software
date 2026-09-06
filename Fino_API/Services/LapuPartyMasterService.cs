using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.LapuPartyMaster;
using FintechApp.Responses.LapuPartyMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class LapuPartyMasterService : ILapuPartyMasterService
    {
        private readonly ILapuPartyMasterRepository _repository;
        public LapuPartyMasterService(ILapuPartyMasterRepository repository) { _repository = repository; }

        private static LapuPartyMasterResponse MapToResponse(LapuPartyMaster e) => new()
        {
            LapuPartyMasterID = e.LapuPartyMasterID,
            PartyName = e.PartyName,
            City = e.City,
            State = e.State,
            Address = e.Address,
            EmailID = e.EmailID,
            MobileNumber = e.MobileNumber,
            IsActive = e.IsActive,
        };

        public async Task<List<LapuPartyMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<LapuPartyMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateLapuPartyMasterRequest request)
        {
            var entity = new LapuPartyMaster
            {
                PartyName = request.PartyName,
                City = request.City,
                State = request.State,
                Address = request.Address,
                EmailID = request.EmailID,
                MobileNumber = request.MobileNumber,
                IsActive = request.IsActive,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateLapuPartyMasterRequest request)
        {
            var entity = await _repository.GetByID(request.LapuPartyMasterID);
            if (entity == null) return Messages.LapuPartyMasterMessages.NotFound;
            entity.PartyName = request.PartyName;
            entity.City = request.City;
            entity.State = request.State;
            entity.Address = request.Address;
            entity.EmailID = request.EmailID;
            entity.MobileNumber = request.MobileNumber;
            entity.IsActive = request.IsActive;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.LapuPartyMasterMessages.NotFound;
        }
    }
}
