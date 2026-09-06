using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.KYC;
using FintechApp.Requests.UserDocumentMaster;
using FintechApp.Responses.KYC;
using FintechApp.Responses.UserDocumentMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserDocumentMasterService : IUserDocumentMasterService
    {
        private readonly IUserDocumentMasterRepository _repository;
        private readonly IKycDocumentMasterRepository _kycDocRepo;
        private readonly IUserRepository _userRepo;
        private readonly IKYCApprovedDetailsRepository _kycApprovedRepo;
        private readonly Logger _logger;

        public UserDocumentMasterService(
            IUserDocumentMasterRepository repository,
            IKycDocumentMasterRepository kycDocRepo,
            IUserRepository userRepo,
            IKYCApprovedDetailsRepository kycApprovedRepo)
        {
            _repository = repository;
            _kycDocRepo = kycDocRepo;
            _userRepo = userRepo;
            _kycApprovedRepo = kycApprovedRepo;
            _logger = new Logger();
        }

        private static UserDocumentMasterResponse MapToResponse(UserDocumentMaster e) => new()
        {
            UserDocumentID = e.UserDocumentID,
            UserID = e.UserID,
            DocmentType = e.DocmentType,
            DocumentNo = e.DocumentNo,
            ImageURL = e.ImageURL,
            Remark = e.Remark,
            Status = e.Status,
            DateTime = e.DateTime
        };

        public async Task<List<UserDocumentMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserDocumentMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserDocumentMasterResponse>> GetByUserID(int userID) { var list = await _repository.GetByUserID(userID); return list.Select(MapToResponse).ToList(); }

        public async Task<string> Create(CreateUserDocumentMasterRequest request)
        {
            var entity = new UserDocumentMaster
            {
                UserID = request.UserID,
                DocmentType = request.DocmentType,
                DocumentNo = request.DocumentNo,
                ImageURL = request.ImageURL,
                Remark = request.Remark,
                Status = request.Status,
                DateTime = DateTime.UtcNow
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserDocumentMasterRequest request)
        {
            var entity = await _repository.GetByID(request.UserDocumentID);
            if (entity == null) return Messages.UserDocumentMasterMessages.NotFound;
            entity.UserID = request.UserID;
            entity.DocmentType = request.DocmentType;
            entity.DocumentNo = request.DocumentNo;
            entity.ImageURL = request.ImageURL;
            entity.Remark = request.Remark;
            entity.Status = request.Status;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.UserDocumentMasterMessages.NotFound;
        }

        public async Task<(string failureMessage, string result)> UploadKYC(UploadKYCRequest request, int userID)
        {
            if (request.KycDocumentMasterID <= 0)
                return ("Invalid document type.", string.Empty);
            if (string.IsNullOrWhiteSpace(request.DocumentNo))
                return ("Invalid document number.", string.Empty);
            if (string.IsNullOrWhiteSpace(request.Document?.Data))
                return ("Invalid document.", string.Empty);

            int targetUserID = request.SelectedUserID > 0 ? request.SelectedUserID : userID;

            var allUsers = await _userRepo.GetAllUsers();
            var user = allUsers.FirstOrDefault(u => u.UserMasterID == targetUserID);
            if (user == null) return ("User not found.", string.Empty);

            var kycDocType = await _kycDocRepo.GetByID(request.KycDocumentMasterID);
            if (kycDocType == null) return ("Invalid document type.", string.Empty);

            string documentType = kycDocType.DocumentType ?? string.Empty;

            var userDocs = await _repository.GetByUserID(targetUserID);
            var existing = userDocs.FirstOrDefault(d => d.DocmentType.Equals(documentType, StringComparison.OrdinalIgnoreCase));
            if (existing != null && existing.Status != null && existing.Status.Equals("APPROVE", StringComparison.OrdinalIgnoreCase))
                return ("Document is already approved.", string.Empty);

            string path = SaveKYCDocument(request.Document, documentType, out string saveError);
            if (!string.IsNullOrWhiteSpace(saveError)) return (saveError, string.Empty);

            if (existing != null)
            {
                existing.DocumentNo = request.DocumentNo;
                existing.ImageURL = path;
                existing.Status = "PROCESS";
                int upd = await _repository.Update(existing);
                if (upd <= 0) return ("Document upload failed.", string.Empty);
            }
            else
            {
                int ins = await _repository.Insert(new UserDocumentMaster
                {
                    UserID = targetUserID,
                    DocmentType = documentType,
                    DocumentNo = request.DocumentNo,
                    ImageURL = path,
                    Status = "PROCESS"
                });
                if (ins <= 0) return ("Document upload failed.", string.Empty);
            }

            return (StatusType.Ok, "KYC document uploaded successfully");
        }

        public async Task<string> ApproveKYC(ApproveKYCRequest request, int adminUserID)
        {
            var allUsers = await _userRepo.GetAllUsers();
            var adminUser = allUsers.FirstOrDefault(u => u.UserMasterID == adminUserID);
            if (adminUser == null || (adminUser.UserTypeID != 1 && adminUser.UserTypeID != 2))
                return Messages.Common.Message.Unauthorized;

            var targetUser = allUsers.FirstOrDefault(u => u.UserMasterID == request.UserID);
            if (targetUser == null) return "User not found.";

            var allDocs = await _repository.GetAll();
            var docRecord = allDocs.FirstOrDefault(d => d.UserDocumentID == request.UserDocumentID);
            if (docRecord == null) return "Document not found.";

            docRecord.Status = request.IsApproved ? "APPROVE" : "REJECT";
            docRecord.Remark = request.Remark;
            int updateResult = await _repository.Update(docRecord);
            if (updateResult <= 0) return "Unable to update.";

            if (request.IsApproved)
            {
                bool changed = false;
                if (string.IsNullOrWhiteSpace(targetUser.AadharNo) || targetUser.AadharNo.Length <= 8)
                    if (docRecord.DocmentType.Contains("AADHAAR", StringComparison.OrdinalIgnoreCase))
                    { targetUser.AadharNo = docRecord.DocumentNo; changed = true; }

                if (string.IsNullOrWhiteSpace(targetUser.PANNo) || targetUser.PANNo.Length <= 7)
                    if (docRecord.DocmentType.Contains("PAN", StringComparison.OrdinalIgnoreCase))
                    { targetUser.PANNo = docRecord.DocumentNo; changed = true; }

                if (string.IsNullOrWhiteSpace(targetUser.GSTNo) || targetUser.GSTNo.Length <= 7)
                    if (docRecord.DocmentType.Contains("GST", StringComparison.OrdinalIgnoreCase))
                    { targetUser.GSTNo = docRecord.DocumentNo; changed = true; }

                if (changed)
                    await _userRepo.UpdateUser(targetUser);

                var kycDocTypes = await _kycDocRepo.GetAll();
                var requiredTypes = kycDocTypes.Where(k => k.IsRequired).ToList();
                var userDocs = await _repository.GetByUserID(request.UserID);

                bool allApproved = requiredTypes.All(required =>
                    userDocs.Any(d => d.DocmentType.Equals(required.DocumentType ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                                   && d.Status != null && d.Status.Equals("APPROVE", StringComparison.OrdinalIgnoreCase)));

                if (allApproved)
                {
                    targetUser.IsKYCApproved = true;
                    await _userRepo.UpdateUser(targetUser);
                    try
                    {
                        await _kycApprovedRepo.Insert(new KYCApprovedDetails
                        {
                            UserID = request.UserID,
                            ApporvedUserID = adminUserID,
                            Remark = request.Remark
                        });
                    }
                    catch (Exception ex) { _logger.Write(ex); }
                }
            }

            return StatusType.Ok;
        }

        public async Task<(string failureMessage, KYCWrapper? data)> SelectKYC(SelectKYCRequest request)
        {
            var allDocs = await _repository.GetAll();
            var allUsers = await _userRepo.GetAllUsers();
            var kycDocTypes = await _kycDocRepo.GetAll();

            var filtered = allDocs.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(request.Status))
                filtered = filtered.Where(d => d.Status != null && d.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            var viewModels = filtered.Select(d =>
            {
                var user = allUsers.FirstOrDefault(u => u.UserMasterID == d.UserID);
                var docType = kycDocTypes.FirstOrDefault(k => k.DocumentType != null && k.DocumentType.Equals(d.DocmentType, StringComparison.OrdinalIgnoreCase));
                return new KYCViewModel
                {
                    UserDocumentID = d.UserDocumentID,
                    UserID = d.UserID,
                    RequestDate = d.DateTime.ToString("dd-MM-yyyy"),
                    PersonName = user?.PersonName ?? string.Empty,
                    Name = user?.FirmName ?? string.Empty,
                    Status = d.Status ?? string.Empty,
                    ISApproved = d.Status != null && d.Status.Equals("APPROVE", StringComparison.OrdinalIgnoreCase),
                    ApprovedImageURL = d.ImageURL,
                    SampleImageURL = docType?.SampleImageURL ?? string.Empty,
                    DocumentType = d.DocmentType,
                    DocumentNo = d.DocumentNo,
                    Remark = d.Remark ?? string.Empty,
                    Label = docType?.Label ?? string.Empty,
                    WhatsAppNumber = user?.WhatsAppNumber ?? string.Empty
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(request.Search))
                viewModels = viewModels.Where(v => v.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                    || v.DocumentNo.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                    || v.DocumentType.Contains(request.Search, StringComparison.OrdinalIgnoreCase)).ToList();

            int count = viewModels.Count;
            if (!request.IsExcelDownload && (request.TakeCount > 0))
                viewModels = viewModels.Skip(request.SkipCount).Take(request.TakeCount).ToList();

            return (StatusType.Ok, new KYCWrapper
            {
                list = viewModels,
                IsExcelDownload = request.IsExcelDownload,
                Count = count
            });
        }

        public async Task<(string failureMessage, List<KYCViewModel> data)> SelectPendingKYC()
        {
            var allDocs = await _repository.GetAll();
            var allUsers = await _userRepo.GetAllUsers();
            var kycDocTypes = await _kycDocRepo.GetAll();

            var pending = allDocs.Where(d => d.Status != null && d.Status.Equals("PROCESS", StringComparison.OrdinalIgnoreCase))
                .Select(d =>
                {
                    var user = allUsers.FirstOrDefault(u => u.UserMasterID == d.UserID);
                    var docType = kycDocTypes.FirstOrDefault(k => k.DocumentType != null && k.DocumentType.Equals(d.DocmentType, StringComparison.OrdinalIgnoreCase));
                    return new KYCViewModel
                    {
                        UserDocumentID = d.UserDocumentID,
                        UserID = d.UserID,
                        RequestDate = d.DateTime.ToString("dd-MM-yyyy"),
                        PersonName = user?.PersonName ?? string.Empty,
                        Name = user?.FirmName ?? string.Empty,
                        Status = d.Status ?? string.Empty,
                        ApprovedImageURL = d.ImageURL,
                        SampleImageURL = docType?.SampleImageURL ?? string.Empty,
                        DocumentType = d.DocmentType,
                        DocumentNo = d.DocumentNo,
                        Label = docType?.Label ?? string.Empty,
                        WhatsAppNumber = user?.WhatsAppNumber ?? string.Empty
                    };
                }).ToList();

            if (!pending.Any())
                return ("No KYC pending.", pending);

            return (StatusType.Ok, pending);
        }

        public async Task<(string failureMessage, List<KYCViewModel> data)> SelectDocsByUserID(int userID)
        {
            var userDocs = await _repository.GetByUserID(userID);
            var kycDocTypes = await _kycDocRepo.GetAll();

            var result = userDocs.Select(d =>
            {
                var docType = kycDocTypes.FirstOrDefault(k => k.DocumentType != null && k.DocumentType.Equals(d.DocmentType, StringComparison.OrdinalIgnoreCase));
                return new KYCViewModel
                {
                    UserDocumentID = d.UserDocumentID,
                    UserID = d.UserID,
                    RequestDate = d.DateTime.ToString("dd-MM-yyyy"),
                    Status = d.Status ?? string.Empty,
                    ISApproved = d.Status != null && d.Status.Equals("APPROVE", StringComparison.OrdinalIgnoreCase),
                    ApprovedImageURL = d.ImageURL,
                    SampleImageURL = docType?.SampleImageURL ?? string.Empty,
                    DocumentType = d.DocmentType,
                    DocumentNo = d.DocumentNo,
                    Remark = d.Remark ?? string.Empty,
                    Label = docType?.Label ?? string.Empty
                };
            }).ToList();

            if (!result.Any())
                return (Messages.Common.Message.DataNotFound, result);

            return (StatusType.Ok, result);
        }

        public async Task<(string failureMessage, KYCViewModel? data)> SelectKYCByID(int userDocumentID)
        {
            var doc = await _repository.GetByID(userDocumentID);
            if (doc == null) return ("Document not found.", null);

            var kycDocTypes = await _kycDocRepo.GetAll();
            var docType = kycDocTypes.FirstOrDefault(k => k.DocumentType != null && k.DocumentType.Equals(doc.DocmentType, StringComparison.OrdinalIgnoreCase));

            var allUsers = await _userRepo.GetAllUsers();
            var user = allUsers.FirstOrDefault(u => u.UserMasterID == doc.UserID);

            return (StatusType.Ok, new KYCViewModel
            {
                UserDocumentID = doc.UserDocumentID,
                UserID = doc.UserID,
                RequestDate = doc.DateTime.ToString("dd-MM-yyyy"),
                Name = user?.FirmName ?? string.Empty,
                PersonName = user?.PersonName ?? string.Empty,
                Status = doc.Status ?? string.Empty,
                ISApproved = doc.Status != null && doc.Status.Equals("APPROVE", StringComparison.OrdinalIgnoreCase),
                ApprovedImageURL = doc.ImageURL,
                SampleImageURL = docType?.SampleImageURL ?? string.Empty,
                DocumentType = doc.DocmentType,
                DocumentNo = doc.DocumentNo,
                Remark = doc.Remark ?? string.Empty,
                Label = docType?.Label ?? string.Empty
            });
        }

        public async Task<(string failureMessage, List<KYCActiveDocViewModel> data)> SelectActiveDocUserwise(int userID)
        {
            var userDocs = await _repository.GetByUserID(userID);
            var kycDocTypes = await _kycDocRepo.GetAll();

            var result = kycDocTypes.Select(docType =>
            {
                var userDoc = userDocs.FirstOrDefault(d => d.DocmentType.Equals(docType.DocumentType ?? string.Empty, StringComparison.OrdinalIgnoreCase));
                return new KYCActiveDocViewModel
                {
                    KycDocumentMasterID = docType.KycDocumentMasterID,
                    DocumentType = docType.DocumentType ?? string.Empty,
                    Label = docType.Label ?? string.Empty,
                    SampleImageURL = docType.SampleImageURL ?? string.Empty,
                    UserDocumentID = userDoc?.UserDocumentID ?? 0,
                    DocumentNo = userDoc?.DocumentNo ?? string.Empty,
                    ApprovedImageURL = userDoc?.ImageURL ?? string.Empty,
                    Status = userDoc?.Status ?? string.Empty
                };
            }).ToList();

            if (!result.Any())
                return (Messages.Common.Message.DataNotFound, result);

            return (StatusType.Ok, result);
        }

        private string SaveKYCDocument(KYCDocumentModel doc, string docType, out string error)
        {
            error = string.Empty;
            try
            {
                string ext = doc.Type.ToLower().Contains("pdf") ? ".pdf"
                           : doc.Type.ToLower().Contains("png") ? ".png"
                           : ".jpg";
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "Media", "KYC", docType);
                Directory.CreateDirectory(folder);
                string filename = $"{Guid.NewGuid():N}{ext}";
                string filepath = Path.Combine(folder, filename);
                byte[] bytes = Convert.FromBase64String(doc.Data);
                File.WriteAllBytes(filepath, bytes);
                return $"Media/KYC/{docType}/{filename}";
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                error = "Document could not be uploaded.";
                return string.Empty;
            }
        }
    }
}
