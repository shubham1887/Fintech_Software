using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SentSMS;
using FintechApp.Responses.SentSMS;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class SentSMSService : ISentSMSService
    {
        private readonly ISentSMSRepository _repository;
        private readonly IUserRepository _userRepository;
        public SentSMSService(ISentSMSRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        // Resolve UserID -> a human label (Firm — Mobile) for the Sent SMS log, so the operator sees
        // WHO the message went to, not a bare id. Built once per list read from the cached user list.
        private async Task<Dictionary<int, string>> BuildUserNameMap()
        {
            var users = await _userRepository.GetAllUsers();
            return users
                .GroupBy(u => u.UserMasterID)
                .ToDictionary(g => g.Key, g =>
                {
                    var u = g.First();
                    string name = !string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName
                                : !string.IsNullOrWhiteSpace(u.PersonName) ? u.PersonName
                                : u.MobileNumber ?? string.Empty;
                    return string.IsNullOrWhiteSpace(u.MobileNumber) || name == u.MobileNumber
                        ? name
                        : $"{name} ({u.MobileNumber})";
                });
        }

        private static SentSMSResponse MapToResponse(SentSMS e) => new()
        {
            SentSMSID = e.SentSMSID,
            UserID = e.UserID,
            Mobile = e.Mobile,
            Message = e.Message,
            Type = e.Type,
            Response = e.Response,
            SMSMasterID = e.SMSMasterID,
            TemplateID = e.TemplateID,
            WhatsAppNumber = e.WhatsAppNumber,
            Request = e.Request,
            DateTime = e.DateTime,
        };

        public async Task<List<SentSMSResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            var names = await BuildUserNameMap();
            return list.OrderByDescending(e => e.SentSMSID).Select(e =>
            {
                var r = MapToResponse(e);
                r.UserName = names.TryGetValue(e.UserID, out var n) ? n : string.Empty;
                return r;
            }).ToList();
        }
        public async Task<SentSMSResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<SentSMSResponse>> GetByUserID(int userid)
        {
            var list = await _repository.GetByUserID(userid);
            var names = await BuildUserNameMap();
            return list.Select(e =>
            {
                var r = MapToResponse(e);
                r.UserName = names.TryGetValue(e.UserID, out var n) ? n : string.Empty;
                return r;
            }).ToList();
        }
        public async Task<string> Create(CreateSentSMSRequest request)
        {
            var entity = new SentSMS
            {
                UserID = request.UserID,
                Mobile = request.Mobile,
                Message = request.Message,
                Type = request.Type,
                Response = request.Response,
                SMSMasterID = request.SMSMasterID,
                TemplateID = request.TemplateID,
                WhatsAppNumber = request.WhatsAppNumber,
                Request = request.Request,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateSentSMSRequest request)
        {
            var entity = await _repository.GetByID(request.SentSMSID);
            if (entity == null) return Messages.SentSMSMessages.NotFound;
            entity.UserID = request.UserID;
            entity.Mobile = request.Mobile;
            entity.Message = request.Message;
            entity.Type = request.Type;
            entity.Response = request.Response;
            entity.SMSMasterID = request.SMSMasterID;
            entity.TemplateID = request.TemplateID;
            entity.WhatsAppNumber = request.WhatsAppNumber;
            entity.Request = request.Request;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.SentSMSMessages.NotFound;
        }
    }
}
