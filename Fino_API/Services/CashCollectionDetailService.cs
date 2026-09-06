using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CashCollectionDetail;
using FintechApp.Responses.CashCollectionDetail;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CashCollectionDetailService : ICashCollectionDetailService
    {
        private readonly ICashCollectionDetailRepository _repository;
        public CashCollectionDetailService(ICashCollectionDetailRepository repository) { _repository = repository; }

        private static CashCollectionDetailResponse MapToResponse(CashCollectionDetail e) => new()
        {
            CashCollectionDetailID = e.CashCollectionDetailID,
            UserID = e.UserID,
            ParentID = e.ParentID,
            OutstandingOpening = e.OutstandingOpening,
            OutstandingAmount = e.OutstandingAmount,
            OutstandingClosing = e.OutstandingClosing,
            ParentRemark = e.ParentRemark,
            PayMode = e.PayMode,
            Credit = e.Credit,
            Debit = e.Debit,
            UTRNumber = e.UTRNumber,
            Type = e.Type,
            IsDelete = e.IsDelete,
        };

        public async Task<List<CashCollectionDetailResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<CashCollectionDetailResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<CashCollectionDetailResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateCashCollectionDetailRequest request)
        {
            var entity = new CashCollectionDetail
            {
                UserID = request.UserID,
                ParentID = request.ParentID,
                OutstandingOpening = request.OutstandingOpening,
                OutstandingAmount = request.OutstandingAmount,
                OutstandingClosing = request.OutstandingClosing,
                ParentRemark = request.ParentRemark,
                PayMode = request.PayMode,
                Credit = request.Credit,
                Debit = request.Debit,
                UTRNumber = request.UTRNumber,
                Type = request.Type,
                IsDelete = request.IsDelete,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateCashCollectionDetailRequest request)
        {
            var entity = await _repository.GetByID(request.CashCollectionDetailID);
            if (entity == null) return Messages.CashCollectionDetailMessages.NotFound;
            entity.UserID = request.UserID;
            entity.ParentID = request.ParentID;
            entity.OutstandingOpening = request.OutstandingOpening;
            entity.OutstandingAmount = request.OutstandingAmount;
            entity.OutstandingClosing = request.OutstandingClosing;
            entity.ParentRemark = request.ParentRemark;
            entity.PayMode = request.PayMode;
            entity.Credit = request.Credit;
            entity.Debit = request.Debit;
            entity.UTRNumber = request.UTRNumber;
            entity.Type = request.Type;
            entity.IsDelete = request.IsDelete;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.CashCollectionDetailMessages.NotFound;
        }
    }
}
