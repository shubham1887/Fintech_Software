using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.EcollectionSetting;
using FintechApp.Responses.EcollectionSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class EcollectionSettingService : IEcollectionSettingService
    {
        private readonly IEcollectionSettingRepository _repository;
        public EcollectionSettingService(IEcollectionSettingRepository repository) { _repository = repository; }

        private static EcollectionSettingResponse MapToResponse(EcollectionSetting e) => new()
        {
            EcollectionSettingID = e.EcollectionSettingID,
            PrivateKeyPath = e.PrivateKeyPath,
            PrivateKeyPassword = e.PrivateKeyPassword,
            PublicKeyPath = e.PublicKeyPath,
            Name = e.Name,
            IsActive = e.IsActive,
            WalletID = e.WalletID,
            Prefix = e.Prefix,
            IFSCCode = e.IFSCCode,
        };

        public async Task<List<EcollectionSettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<EcollectionSettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateEcollectionSettingRequest request)
        {
            var entity = new EcollectionSetting
            {
                PrivateKeyPath = request.PrivateKeyPath,
                PrivateKeyPassword = request.PrivateKeyPassword,
                PublicKeyPath = request.PublicKeyPath,
                Name = request.Name,
                IsActive = request.IsActive,
                WalletID = request.WalletID,
                Prefix = request.Prefix,
                IFSCCode = request.IFSCCode,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateEcollectionSettingRequest request)
        {
            var entity = await _repository.GetByID(request.EcollectionSettingID);
            if (entity == null) return Messages.EcollectionSettingMessages.NotFound;
            entity.PrivateKeyPath = request.PrivateKeyPath;
            entity.PrivateKeyPassword = request.PrivateKeyPassword;
            entity.PublicKeyPath = request.PublicKeyPath;
            entity.Name = request.Name;
            entity.IsActive = request.IsActive;
            entity.WalletID = request.WalletID;
            entity.Prefix = request.Prefix;
            entity.IFSCCode = request.IFSCCode;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.EcollectionSettingMessages.NotFound;
        }
    }
}
