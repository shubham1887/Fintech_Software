using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SubMode;
using FintechApp.Responses.SubMode;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class SubModeService : ISubModeService
    {
        private readonly ISubModeRepository _repository;
        private readonly IModeRepository _modeRepository;

        public SubModeService(ISubModeRepository repository, IModeRepository modeRepository)
        {
            _repository = repository;
            _modeRepository = modeRepository;
        }

        public async Task<List<SubModeResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            var modes = (await _modeRepository.GetAll()).ToDictionary(m => m.ModeID, m => m.ModeName);
            return all.Select(x => MapToResponse(x, modes)).ToList();
        }

        public async Task<List<SubModeResponse>> GetByModeID(int modeId)
        {
            var list = await _repository.GetByModeID(modeId);
            var modes = (await _modeRepository.GetAll()).ToDictionary(m => m.ModeID, m => m.ModeName);
            return list.Where(x => x.IsActive).Select(x => MapToResponse(x, modes)).ToList();
        }

        public async Task<SubModeResponse?> GetByID(int subModeId)
        {
            var entity = await _repository.GetByID(subModeId);
            if (entity == null) return null;
            var modes = (await _modeRepository.GetAll()).ToDictionary(m => m.ModeID, m => m.ModeName);
            return MapToResponse(entity, modes);
        }

        public async Task<string> Create(CreateSubModeRequest request, int userId)
        {
            var mode = await _modeRepository.GetByID(request.ModeID);
            if (mode == null) return Messages.SubModeMessages.InvalidMode;

            if (await _repository.ExistsByName(request.ModeID, request.SubModeName, 0))
                return Messages.SubModeMessages.AlreadyExists;

            var entity = new SubMode
            {
                ModeID = request.ModeID,
                SubModeName = request.SubModeName.Trim(),
                Balance = request.Balance,
                BankName = request.BankName?.Trim() ?? string.Empty,
                IFSC = request.IFSC?.Trim() ?? string.Empty,
                BeneficiaryName = request.BeneficiaryName?.Trim() ?? string.Empty,
                AccountNumber = request.AccountNumber?.Trim() ?? string.Empty,
                QRCode = request.QRCode?.Trim() ?? string.Empty,
                QRImage = request.QRImage?.Trim() ?? string.Empty,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                IsDelete = false,
                CreatedDate = DateTime.Now,
                CreatedBy = userId
            };
            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateSubModeRequest request, int userId)
        {
            var entity = await _repository.GetByID(request.SubModeID);
            if (entity == null) return Messages.SubModeMessages.NotFound;

            var mode = await _modeRepository.GetByID(request.ModeID);
            if (mode == null) return Messages.SubModeMessages.InvalidMode;

            if (await _repository.ExistsByName(request.ModeID, request.SubModeName, request.SubModeID))
                return Messages.SubModeMessages.AlreadyExists;

            entity.ModeID = request.ModeID;
            entity.SubModeName = request.SubModeName.Trim();
            entity.Balance = request.Balance;
            entity.BankName = request.BankName?.Trim() ?? string.Empty;
            entity.IFSC = request.IFSC?.Trim() ?? string.Empty;
            entity.BeneficiaryName = request.BeneficiaryName?.Trim() ?? string.Empty;
            entity.AccountNumber = request.AccountNumber?.Trim() ?? string.Empty;
            entity.QRCode = request.QRCode?.Trim() ?? string.Empty;
            entity.QRImage = request.QRImage?.Trim() ?? string.Empty;
            entity.IsActive = request.IsActive;
            entity.DisplayOrder = request.DisplayOrder;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedBy = userId;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int subModeId)
        {
            var entity = await _repository.GetByID(subModeId);
            if (entity == null) return Messages.SubModeMessages.NotFound;
            int result = await _repository.Delete(subModeId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static SubModeResponse MapToResponse(SubMode e, IReadOnlyDictionary<int, string> modeNames) => new()
        {
            SubModeID = e.SubModeID,
            ModeID = e.ModeID,
            ModeName = modeNames.TryGetValue(e.ModeID, out var name) ? name : string.Empty,
            SubModeName = e.SubModeName,
            Balance = e.Balance,
            BankName = e.BankName,
            IFSC = e.IFSC,
            BeneficiaryName = e.BeneficiaryName,
            AccountNumber = e.AccountNumber,
            QRCode = e.QRCode,
            QRImage = e.QRImage,
            IsActive = e.IsActive,
            DisplayOrder = e.DisplayOrder,
            CreatedDate = e.CreatedDate
        };
    }
}
