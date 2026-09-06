using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.FundTransaction;
using FintechApp.Responses.FundTransaction;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class FundTransactionService : IFundTransactionService
    {
        private readonly IFundTransactionRepository _fundTransactionRepository;

        public FundTransactionService(IFundTransactionRepository fundTransactionRepository)
        {
            _fundTransactionRepository = fundTransactionRepository;
        }

        public async Task<FundTransactionResponse?> GetFundTransactionById(int fundTransactionId)
        {
            var transaction = await _fundTransactionRepository.GetFundTransactionById(fundTransactionId);
            if (transaction == null) return null;
            return MapToResponse(transaction);
        }

        public async Task<List<FundTransactionResponse>> GetFundTransactions(GetFundTransactionRequest request)
        {
            var transactions = await _fundTransactionRepository.GetFundTransactions(request);
            return transactions.Select(MapToResponse).ToList();
        }

        public async Task<string> CreateFundTransaction(CreateFundTransactionRequest request)
        {
            var fundTransaction = new FundTransaction
            {
                SenderID = request.SenderID,
                ReceiverID = request.ReceiverID,
                WalletID = request.WalletID,
                Amount = request.Amount,
                OriginalAmount = request.OriginalAmount,
                CashCollectionAmount = request.CashCollectionAmount,
                SenderOpeningBalance = request.SenderOpeningBalance,
                SenderClosingBalance = request.SenderClosingBalance,
                ReceiverOpeningBalance = request.ReceiverOpeningBalance,
                ReceiverClosingBalance = request.ReceiverClosingBalance,
                Remark = request.Remark,
                UTRNumber = request.UTRNumber,
                Status = request.Status,
                Type = request.Type,
                AdminRemark = request.AdminRemark,
                PayMode = request.PayMode,
                Through = request.Through,
                RequestToID = request.RequestToID,
                CashCollectionRemark = request.CashCollectionRemark,
                Description = request.Description,
                BankName = request.BankName,
                ReceiptPath = request.ReceiptPath,
                TransactionDOC = request.TransactionDOC,
                TransactionDOCInt = request.TransactionDOCInt,
                RequestAcceptedUserID = request.RequestAcceptedUserID,
                IsDelete = false
            };

            int result = await _fundTransactionRepository.Insert(fundTransaction);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> UpdateFundTransaction(UpdateFundTransactionRequest request)
        {
            var fundTransaction = await _fundTransactionRepository.GetFundTransactionById(request.FundTransactionID);
            if (fundTransaction == null)
                return FintechApp.Messages.FundTransactionMessages.FundTransactionNotFound;

            switch (request.FieldName.ToLower())
            {
                case "status":
                    fundTransaction.Status = request.NewValue;
                    break;

                case "adminremark":
                    fundTransaction.AdminRemark = request.NewValue;
                    break;

                case "remark":
                    fundTransaction.Remark = request.NewValue;
                    break;

                case "utrnumber":
                    fundTransaction.UTRNumber = request.NewValue;
                    break;

                case "paymode":
                    fundTransaction.PayMode = request.NewValue;
                    break;

                case "through":
                    fundTransaction.Through = request.NewValue;
                    break;

                case "cashcollectionremark":
                    fundTransaction.CashCollectionRemark = request.NewValue;
                    break;

                case "description":
                    fundTransaction.Description = request.NewValue;
                    break;

                case "bankname":
                    fundTransaction.BankName = request.NewValue;
                    break;

                case "receiptpath":
                    fundTransaction.ReceiptPath = request.NewValue;
                    break;

                case "requestaccepteduserid":
                    if (!int.TryParse(request.NewValue, out int acceptedUserId))
                        return FintechApp.Messages.FundTransactionMessages.InvalidValue;
                    fundTransaction.RequestAcceptedUserID = acceptedUserId;
                    break;

                default:
                    return FintechApp.Messages.FundTransactionMessages.InvalidFieldName;
            }

            int result = await _fundTransactionRepository.Update(fundTransaction);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> DeleteFundTransaction(int fundTransactionId)
        {
            var fundTransaction = await _fundTransactionRepository.GetFundTransactionById(fundTransactionId);
            if (fundTransaction == null)
                return FintechApp.Messages.FundTransactionMessages.FundTransactionNotFound;

            fundTransaction.IsDelete = true;

            int result = await _fundTransactionRepository.Delete(fundTransaction);
            return result > 0 ? StatusType.Ok : FintechApp.Messages.Common.Message.ExceptionOccured;
        }

        private static FundTransactionResponse MapToResponse(FundTransaction f)
        {
            return new FundTransactionResponse
            {
                FundTransactionID = f.FundTransactionID,
                SenderID = f.SenderID,
                SenderName = f.Sender?.PersonName ?? string.Empty,
                ReceiverID = f.ReceiverID,
                ReceiverName = f.Receiver?.PersonName ?? string.Empty,
                WalletID = f.WalletID,
                WalletName = f.WalletMaster?.Name ?? string.Empty,
                Amount = f.Amount,
                OriginalAmount = f.OriginalAmount,
                CashCollectionAmount = f.CashCollectionAmount,
                SenderOpeningBalance = f.SenderOpeningBalance,
                SenderClosingBalance = f.SenderClosingBalance,
                ReceiverOpeningBalance = f.ReceiverOpeningBalance,
                ReceiverClosingBalance = f.ReceiverClosingBalance,
                Remark = f.Remark ?? string.Empty,
                UTRNumber = f.UTRNumber ?? string.Empty,
                Status = f.Status ?? string.Empty,
                Type = f.Type ?? string.Empty,
                AdminRemark = f.AdminRemark ?? string.Empty,
                PayMode = f.PayMode ?? string.Empty,
                Through = f.Through ?? string.Empty,
                RequestToID = f.RequestToID,
                RequestToName = f.RequestTo?.PersonName ?? string.Empty,
                CashCollectionRemark = f.CashCollectionRemark ?? string.Empty,
                Description = f.Description ?? string.Empty,
                BankName = f.BankName ?? string.Empty,
                ReceiptPath = f.ReceiptPath ?? string.Empty,
                TransactionDOC = f.TransactionDOC,
                TransactionDOCInt = f.TransactionDOCInt,
                IsDelete = f.IsDelete,
                RequestAcceptedUserID = f.RequestAcceptedUserID
            };
        }
    }
}