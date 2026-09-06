using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.TransactionMaster;
using FintechApp.Responses.TransactionMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class TransactionMasterService : ITransactionMasterService
    {
        private readonly ITransactionMasterRepository _repository;

        public TransactionMasterService(ITransactionMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TransactionResponse>> GetAll(GetTransactionRequest request)
        {
            var all = await _repository.GetAll(
                request.UserID,
                request.Status,
                request.ServiceType,
                request.FromDate,
                request.ToDate,
                request.PageNumber,
                request.PageSize
            );

            return all.Select(MapToResponse).ToList();
        }

        public async Task<TransactionResponse?> GetByID(int transactionMasterID)
        {
            var entity = await _repository.GetByID(transactionMasterID);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateTransactionRequest request)
        {
            var existing = await _repository.GetByProviderRequestID(request.ProviderRequestID);
            if (existing != null) return TransactionMasterMessages.AlreadyExists;

            var entity = new TransactionMaster
            {
                UserID = request.UserID,
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                WalletID = request.WalletID,
                ProviderID = request.ProviderID,
                DistributerUserID = request.DistributerUserID,
                SuperDistributerUserID = request.SuperDistributerUserID,
                ConsumerNumber = request.ConsumerNumber,
                Amount = request.Amount,
                DebitAmount = request.DebitAmount,
                OpeningBalance = request.OpeningBalance,
                ClosingBalance = request.ClosingBalance,
                Status = request.Status.ToUpperInvariant(),
                Description = request.Description,
                ProviderRequestID = request.ProviderRequestID,
                ReferenceNo = request.ReferenceNo,
                Through = request.Through,
                IPAddress = request.IPAddress,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> UpdateStatus(UpdateTransactionStatusRequest request)
        {
            var entity = await _repository.GetByID(request.TransactionID);
            if (entity == null) return TransactionMasterMessages.NotFound;

            int result = await _repository.UpdateStatus(request.TransactionID, request.Status.ToUpperInvariant(), request.TxnID, request.Remark);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        private static TransactionResponse MapToResponse(TransactionMaster x) => new()
        {
            TransactionMasterID = x.TransactionMasterID,
            UserID = x.UserID,
            ServiceID = x.ServiceID,
            OperatorID = x.OperatorID,
            WalletID = x.WalletID,
            ProviderID = x.ProviderID,
            DistributerUserID = x.DistributerUserID,
            SuperDistributerUserID = x.SuperDistributerUserID,
            ConsumerNumber = x.ConsumerNumber,
            Amount = x.Amount,
            DebitAmount = x.DebitAmount,
            OpeningBalance = x.OpeningBalance,
            ClosingBalance = x.ClosingBalance,
            Status = x.Status,
            Description = x.Description,
            ProviderRequestID = x.ProviderRequestID,
            ReferenceNo = x.ReferenceNo,
            APIUserRequestID = x.APIUserRequestID,
            ProviderDOCNo = x.ProviderDOCNo,
            ProviderRefNo = x.ProviderRefNo,
            RetailerCommission = x.RetailerCommission,
            DistributerCommission = x.DistributerCommission,
            SuperDistributerCommission = x.SuperDistributerCommission,
            AdminCommission = x.AdminCommission,
            ProviderCommission = x.ProviderCommission,
            RetailerTDS = x.RetailerTDS,
            DistributerTDS = x.DistributerTDS,
            SuperDistributerTDS = x.SuperDistributerTDS,
            AdminGST = x.AdminGST,
            RetailerGST = x.RetailerGST,
            DistributerGST = x.DistributerGST,
            SuperDistributerGST = x.SuperDistributerGST,
            CashBack = x.CashBack,
            NetCommission = x.NetCommission,
            Charge = x.Charge,
            AdminProfit = x.AdminProfit,
            DMT_SenderID = x.DMT_SenderID,
            DMT_SenderName = x.DMT_SenderName,
            DMT_SenderNumber = x.DMT_SenderNumber,
            DMT_BeneficiaryID = x.DMT_BeneficiaryID,
            DMT_BeneficiaryName = x.DMT_BeneficiaryName,
            DMT_Bank = x.DMT_Bank,
            DMT_IFSC = x.DMT_IFSC,
            DMT_BeneficaryNameByBank = x.DMT_BeneficaryNameByBank,
            DMT_PaymentType = x.DMT_PaymentType,
            DMT_OriginalAmount = x.DMT_OriginalAmount,
            ErrorCode = x.ErrorCode,
            Location = x.Location,
            Through = x.Through,
            IPAddress = x.IPAddress,
            Circle = x.Circle,
            ComplaintStatus = x.ComplaintStatus,
            CardType = x.CardType,
            ChangeStatus = x.ChangeStatus,
            PendingProviderStatus = x.PendingProviderStatus,
            ResponseType = x.ResponseType,
            BBPS_CustomerName = x.BBPS_CustomerName,
            IsDispute = x.IsDispute,
            IsROffer = x.IsROffer,
            IsOnlineDMT = x.IsOnlineDMT,
            IsCCF = x.IsCCF,
            SettlementAtProviderEnd = x.SettlementAtProviderEnd,
            HitBackCount = x.HitBackCount,
            StateID = x.StateID,
            DateTime = x.DateTime,
            UpdatedateTime = x.UpdatedateTime
        };
    }
}
