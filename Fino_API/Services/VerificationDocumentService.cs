using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Verification;
using FintechApp.Responses.Verification;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// Config-driven verification documents: admin manages the master list; users submit against
    /// whatever is active. Nothing about the document set is hard-coded — new regulatory documents
    /// are added from the admin panel and appear on the user's Manual Verification screen at once.
    /// </summary>
    public class VerificationDocumentService : IVerificationDocumentService
    {
        private readonly IVerificationDocumentConfigRepository _repository;
        private readonly IUserDocumentMasterRepository _userDocumentRepository;

        public VerificationDocumentService(
            IVerificationDocumentConfigRepository repository,
            IUserDocumentMasterRepository userDocumentRepository)
        {
            _repository = repository;
            _userDocumentRepository = userDocumentRepository;
        }

        private static VerificationDocumentResponse Map(VerificationDocumentConfig e) => new()
        {
            VerificationDocumentConfigID = e.VerificationDocumentConfigID,
            DocumentName = e.DocumentName,
            DocumentCode = e.DocumentCode,
            DocumentType = e.DocumentType,
            IsRequired = e.IsRequired,
            IsActive = e.IsActive,
            DisplayOrder = e.DisplayOrder,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate,
        };

        public async Task<List<VerificationDocumentResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(Map).ToList();
        }

        public async Task<string> Create(CreateVerificationDocumentRequest request, int currentUserID)
        {
            if (string.IsNullOrWhiteSpace(request.DocumentName)) return "Document name is required.";
            var entity = new VerificationDocumentConfig
            {
                DocumentName = request.DocumentName.Trim(),
                DocumentCode = (string.IsNullOrWhiteSpace(request.DocumentCode)
                    ? request.DocumentName.Trim().ToUpperInvariant().Replace(" ", "_")
                    : request.DocumentCode.Trim().ToUpperInvariant()),
                DocumentType = request.DocumentType?.Trim() ?? string.Empty,
                IsRequired = request.IsRequired,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                CreatedBy = currentUserID,
                CreatedDate = DateTime.UtcNow,
            };
            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : "Could not add the document.";
        }

        public async Task<string> Update(UpdateVerificationDocumentRequest request, int currentUserID)
        {
            var entity = await _repository.GetByID(request.VerificationDocumentConfigID);
            if (entity == null) return "Document not found.";
            // GetAll is AsNoTracking — build a detached instance carrying the PK and new values.
            var updated = new VerificationDocumentConfig
            {
                VerificationDocumentConfigID = entity.VerificationDocumentConfigID,
                DocumentName = string.IsNullOrWhiteSpace(request.DocumentName) ? entity.DocumentName : request.DocumentName.Trim(),
                DocumentCode = string.IsNullOrWhiteSpace(request.DocumentCode) ? entity.DocumentCode : request.DocumentCode.Trim().ToUpperInvariant(),
                DocumentType = request.DocumentType?.Trim() ?? entity.DocumentType,
                IsRequired = request.IsRequired,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,
                UpdatedBy = currentUserID,
                UpdatedDate = DateTime.UtcNow,
            };
            int result = await _repository.Update(updated);
            return result > 0 ? StatusType.Ok : "Could not update the document.";
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : "Could not delete the document.";
        }

        public async Task<List<VerificationDocumentResponse>> GetRequiredDocuments(int currentUserID)
        {
            var active = (await _repository.GetAll()).Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder).ThenBy(x => x.VerificationDocumentConfigID).ToList();
            var userDocs = await _userDocumentRepository.GetByUserID(currentUserID);
            return active.Select(e =>
            {
                var vm = Map(e);
                // Match the user's stored document by the document code (its DocmentType).
                var mine = userDocs.FirstOrDefault(d => (d.DocmentType ?? string.Empty)
                    .Equals(e.DocumentCode, StringComparison.OrdinalIgnoreCase));
                if (mine != null)
                {
                    vm.Status = mine.Status ?? string.Empty;
                    vm.DocumentNo = mine.DocumentNo ?? string.Empty;
                    vm.ImageURL = mine.ImageURL ?? string.Empty;
                    // Without the remark a rejected document tells the user nothing about what to
                    // fix — the reason the admin typed is the whole point of the rejection.
                    vm.Remark = mine.Remark ?? string.Empty;
                }
                return vm;
            }).ToList();
        }

        public async Task<string> SubmitDocument(SubmitVerificationDocumentRequest request, int currentUserID)
        {
            var config = await _repository.GetByID(request.VerificationDocumentConfigID);
            if (config == null || !config.IsActive) return "Invalid document.";
            if (string.IsNullOrWhiteSpace(request.ImageURL)) return "Please upload the document image.";
            if (string.IsNullOrWhiteSpace(request.DocumentNo)) return "Please enter the document number.";

            var userDocs = await _userDocumentRepository.GetByUserID(currentUserID);
            var existing = userDocs.FirstOrDefault(d => (d.DocmentType ?? string.Empty)
                .Equals(config.DocumentCode, StringComparison.OrdinalIgnoreCase));
            if (existing != null && (existing.Status ?? string.Empty).Equals("APPROVE", StringComparison.OrdinalIgnoreCase))
                return "This document is already approved.";

            if (existing != null)
            {
                existing.DocumentNo = request.DocumentNo.Trim();
                existing.ImageURL = request.ImageURL;
                existing.Status = "PROCESS";
                existing.UpdatedateTime = DateTime.UtcNow;
                int upd = await _userDocumentRepository.Update(existing);
                return upd > 0 ? StatusType.Ok : "Could not save the document.";
            }

            int ins = await _userDocumentRepository.Insert(new UserDocumentMaster
            {
                UserID = currentUserID,
                DocmentType = config.DocumentCode,
                DocumentNo = request.DocumentNo.Trim(),
                ImageURL = request.ImageURL,
                Status = "PROCESS",
                DateTime = DateTime.UtcNow,
            });
            return ins > 0 ? StatusType.Ok : "Could not save the document.";
        }
    }
}
