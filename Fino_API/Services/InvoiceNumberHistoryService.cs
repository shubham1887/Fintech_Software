using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.InvoiceNumberHistory;
using FintechApp.Responses.InvoiceNumberHistory;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class InvoiceNumberHistoryService : IInvoiceNumberHistoryService
    {
        private readonly IInvoiceNumberHistoryRepository _repository;
        public InvoiceNumberHistoryService(IInvoiceNumberHistoryRepository repository) { _repository = repository; }

        private static InvoiceNumberHistoryResponse MapToResponse(InvoiceNumberHistory e) => new()
        {
            InvoiceNumberHistoryID = e.InvoiceNumberHistoryID,
            ServiceCategory = e.ServiceCategory,
            UserID = e.UserID,
            Month = e.Month,
            Year = e.Year,
            InvoiceNo = e.InvoiceNo,
            InvoiceType = e.InvoiceType,
            IncrementValue = e.IncrementValue,
        };

        public async Task<List<InvoiceNumberHistoryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<InvoiceNumberHistoryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<InvoiceNumberHistoryResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateInvoiceNumberHistoryRequest request)
        {
            var entity = new InvoiceNumberHistory
            {
                ServiceCategory = request.ServiceCategory,
                UserID = request.UserID,
                Month = request.Month,
                Year = request.Year,
                InvoiceNo = request.InvoiceNo,
                InvoiceType = request.InvoiceType,
                IncrementValue = request.IncrementValue,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateInvoiceNumberHistoryRequest request)
        {
            var entity = await _repository.GetByID(request.InvoiceNumberHistoryID);
            if (entity == null) return Messages.InvoiceNumberHistoryMessages.NotFound;
            entity.ServiceCategory = request.ServiceCategory;
            entity.UserID = request.UserID;
            entity.Month = request.Month;
            entity.Year = request.Year;
            entity.InvoiceNo = request.InvoiceNo;
            entity.InvoiceType = request.InvoiceType;
            entity.IncrementValue = request.IncrementValue;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.InvoiceNumberHistoryMessages.NotFound;
        }
    }
}
