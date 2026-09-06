using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.InvoiceSetting;
using FintechApp.Responses.InvoiceSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class InvoiceSettingService : IInvoiceSettingService
    {
        private readonly IInvoiceSettingRepository _repository;

        public InvoiceSettingService(IInvoiceSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<InvoiceSettingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<InvoiceSettingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateInvoiceSettingRequest request)
        {
            var entity = new InvoiceSetting
            {
                CompanyName = request.CompanyName,
                LogoURL = request.LogoURL,
                CompanyPrefix = request.CompanyPrefix,
                Address = request.Address,
                ShortAddress = request.ShortAddress,
                PinCode = request.PinCode,
                District = request.District,
                Country = request.Country,
                GSTIN = request.GSTIN,
                PAN = request.PAN,
                CIN = request.CIN,
                State = request.State,
                AccountNo = request.AccountNo,
                AccountType = request.AccountType,
                IFSC = request.IFSC,
                AccountHolderName = request.AccountHolderName,
                BankName = request.BankName,
                BranchName = request.BranchName,
                Email = request.Email,
                WebsiteName = request.WebsiteName,
                Note = request.Note,
                FullAddress = request.FullAddress,
                SignatureURL = request.SignatureURL
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateInvoiceSettingRequest request)
        {
            var entity = await _repository.GetByID(request.InvoiceSettingID);
            if (entity == null) return InvoiceSettingMessages.NotFound;
            entity.CompanyName = request.CompanyName;
            entity.LogoURL = request.LogoURL;
            entity.CompanyPrefix = request.CompanyPrefix;
            entity.Address = request.Address;
            entity.ShortAddress = request.ShortAddress;
            entity.PinCode = request.PinCode;
            entity.District = request.District;
            entity.Country = request.Country;
            entity.GSTIN = request.GSTIN;
            entity.PAN = request.PAN;
            entity.CIN = request.CIN;
            entity.State = request.State;
            entity.AccountNo = request.AccountNo;
            entity.AccountType = request.AccountType;
            entity.IFSC = request.IFSC;
            entity.AccountHolderName = request.AccountHolderName;
            entity.BankName = request.BankName;
            entity.BranchName = request.BranchName;
            entity.Email = request.Email;
            entity.WebsiteName = request.WebsiteName;
            entity.Note = request.Note;
            entity.FullAddress = request.FullAddress;
            entity.SignatureURL = request.SignatureURL;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : InvoiceSettingMessages.NotFound;
        }

        private static InvoiceSettingResponse MapToResponse(InvoiceSetting e) => new()
        {
            InvoiceSettingID = e.InvoiceSettingID,
            CompanyName = e.CompanyName,
            LogoURL = e.LogoURL,
            CompanyPrefix = e.CompanyPrefix,
            Address = e.Address,
            ShortAddress = e.ShortAddress,
            PinCode = e.PinCode,
            District = e.District,
            Country = e.Country,
            GSTIN = e.GSTIN,
            PAN = e.PAN,
            CIN = e.CIN,
            State = e.State,
            AccountNo = e.AccountNo,
            AccountType = e.AccountType,
            IFSC = e.IFSC,
            AccountHolderName = e.AccountHolderName,
            BankName = e.BankName,
            BranchName = e.BranchName,
            Email = e.Email,
            WebsiteName = e.WebsiteName,
            Note = e.Note,
            FullAddress = e.FullAddress,
            SignatureURL = e.SignatureURL
        };
    }
}
