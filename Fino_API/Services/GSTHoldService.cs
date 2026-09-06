using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.GSTHold;
using FintechApp.Responses.GSTHold;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class GSTHoldService : IGSTHoldService
    {
        private readonly IGSTHoldRepository _repository;
        public GSTHoldService(IGSTHoldRepository repository) { _repository = repository; }

        private static GSTHoldResponse MapToResponse(GSTHold e) => new()
        {
            GSTHoldID = e.GSTHoldID,
            UserID = e.UserID,
            Month = e.Month,
            Year = e.Year,
            HoldAmount = e.HoldAmount,
            GSTUploadedDate = e.GSTUploadedDate,
            ReceiptPath = e.ReceiptPath,
            Status = e.Status,
            GSTUploadRemark = e.GSTUploadRemark,
            AdminRemark = e.AdminRemark,
            GSTNo = e.GSTNo,
            StateID = e.StateID,
            TaxableValue = e.TaxableValue,
            IGST = e.IGST,
            CGST = e.CGST,
            SGST = e.SGST,
        };

        public async Task<List<GSTHoldResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<GSTHoldResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<GSTHoldResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateGSTHoldRequest request)
        {
            var entity = new GSTHold
            {
                UserID = request.UserID,
                Month = request.Month,
                Year = request.Year,
                HoldAmount = request.HoldAmount,
                GSTUploadedDate = request.GSTUploadedDate,
                ReceiptPath = request.ReceiptPath,
                Status = request.Status,
                GSTUploadRemark = request.GSTUploadRemark,
                AdminRemark = request.AdminRemark,
                GSTNo = request.GSTNo,
                StateID = request.StateID,
                TaxableValue = request.TaxableValue,
                IGST = request.IGST,
                CGST = request.CGST,
                SGST = request.SGST,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateGSTHoldRequest request)
        {
            var entity = await _repository.GetByID(request.GSTHoldID);
            if (entity == null) return Messages.GSTHoldMessages.NotFound;
            entity.UserID = request.UserID;
            entity.Month = request.Month;
            entity.Year = request.Year;
            entity.HoldAmount = request.HoldAmount;
            entity.GSTUploadedDate = request.GSTUploadedDate;
            entity.ReceiptPath = request.ReceiptPath;
            entity.Status = request.Status;
            entity.GSTUploadRemark = request.GSTUploadRemark;
            entity.AdminRemark = request.AdminRemark;
            entity.GSTNo = request.GSTNo;
            entity.StateID = request.StateID;
            entity.TaxableValue = request.TaxableValue;
            entity.IGST = request.IGST;
            entity.CGST = request.CGST;
            entity.SGST = request.SGST;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.GSTHoldMessages.NotFound;
        }
    }
}
