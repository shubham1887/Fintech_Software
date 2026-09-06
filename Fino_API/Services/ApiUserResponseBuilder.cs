using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Responses.ApiUser;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IApiUserResponseBuilder"/>
    public class ApiUserResponseBuilder : IApiUserResponseBuilder
    {
        /// <summary>The one date format the contract publishes. Do not vary it per endpoint.</summary>
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        private readonly IOperatorRepository _operatorRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IUserWalletMasterRepository _userWalletRepo;
        private readonly IWalletMasterRepository _walletMasterRepo;
        private readonly Logger _logger = new();

        public ApiUserResponseBuilder(
            IOperatorRepository operatorRepo,
            IServiceRepository serviceRepo,
            IUserWalletMasterRepository userWalletRepo,
            IWalletMasterRepository walletMasterRepo)
        {
            _operatorRepo = operatorRepo;
            _serviceRepo = serviceRepo;
            _userWalletRepo = userWalletRepo;
            _walletMasterRepo = walletMasterRepo;
        }

        public async Task<ApiUserTransactionResponse> FromTransaction(
            TransactionMaster txn,
            string eventType,
            decimal? balanceOverride = null,
            string? messageOverride = null,
            bool duplicate = false,
            string? requestIdOverride = null)
        {
            string status = ApiUserStatus.Normalize(txn.Status);
            string message = !string.IsNullOrWhiteSpace(messageOverride)
                ? messageOverride!
                : (!string.IsNullOrWhiteSpace(txn.Description) ? txn.Description : DefaultMessage(status));

            var response = ApiUserResponseFactory.Apply(
                new ApiUserTransactionResponse(),
                status,
                ApiUserErrorCode.None,
                message,
                requestIdOverride ?? txn.APIUserRequestID);

            response.Event = eventType;
            response.Final = ApiUserStatus.IsFinal(status);
            response.Duplicate = duplicate;
            response.TxnId = txn.TransactionMasterID;
            response.ProviderRef = string.IsNullOrWhiteSpace(txn.ProviderRefNo) ? txn.ReferenceNo : txn.ProviderRefNo;
            response.ServiceId = txn.ServiceID;
            response.Number = txn.ConsumerNumber;
            response.Amount = txn.Amount;
            response.DebitAmount = txn.DebitAmount;
            response.Date = txn.DateTime.ToString(DateFormat);

            // Names are cosmetic — a lookup failure must never cost the integrator their result.
            try
            {
                var op = txn.OperatorID > 0 ? await _operatorRepo.GetOperatorById(txn.OperatorID) : null;
                response.Operator = op?.OperatorName ?? string.Empty;
                response.OperatorCode = op?.Opcode ?? string.Empty;

                var svc = txn.ServiceID > 0 ? await _serviceRepo.GetServiceById(txn.ServiceID) : null;
                response.Service = svc?.Name ?? string.Empty;
            }
            catch (Exception ex) { _logger.Write(ex); }

            response.Balance = balanceOverride ?? await ResolveBalance(txn.UserID, txn.WalletID, txn.ClosingBalance);
            return response;
        }

        public async Task<ApiUserBalanceResponse> Balance(int userId, string requestId)
        {
            var wallets = await _userWalletRepo.GetByUserID(userId);
            var names = (await _walletMasterRepo.SelectAll()).ToDictionary(w => w.WalletID, w => w.Name);

            var rows = wallets
                .OrderBy(w => w.WalletID)
                .Select(w => new ApiUserWalletBalance
                {
                    WalletId = w.WalletID,
                    Wallet = names.TryGetValue(w.WalletID, out var nm) ? nm : $"Wallet {w.WalletID}",
                    Balance = w.Balance,
                    Hold = w.HoldBalance,
                    Available = w.Balance - w.HoldBalance,
                })
                .ToList();

            var response = ApiUserResponseFactory.Ok(new ApiUserBalanceResponse(), "Balance fetched successfully.", requestId);
            response.Wallets = rows;
            response.Balance = rows.Sum(r => r.Available);
            return response;
        }

        public async Task<ApiUserOperatorListResponse> Operators(string requestId)
        {
            var ops = await _operatorRepo.GetActiveOperators();
            var serviceNames = (await _serviceRepo.GetAllServices()).ToDictionary(s => s.ServiceID, s => s.Name);

            var rows = ops
                .Where(o => !string.IsNullOrWhiteSpace(o.Opcode))
                .OrderBy(o => o.ServiceID).ThenBy(o => o.OperatorName)
                .Select(o => new ApiUserOperator
                {
                    Code = o.Opcode,
                    Name = o.OperatorName,
                    ServiceId = o.ServiceID,
                    Service = serviceNames.TryGetValue(o.ServiceID, out var sn) ? sn : string.Empty,
                })
                .ToList();

            var response = ApiUserResponseFactory.Ok(new ApiUserOperatorListResponse(), "Operator list fetched successfully.", requestId);
            response.Operators = rows;
            response.Count = rows.Count;
            return response;
        }

        /// <summary>Live wallet balance, falling back to the balance stamped on the transaction.</summary>
        private async Task<decimal> ResolveBalance(int userId, int walletId, decimal fallback)
        {
            try
            {
                var wallet = await _userWalletRepo.GetByUserAndWallet(userId, walletId);
                if (wallet != null) return wallet.Balance;
            }
            catch (Exception ex) { _logger.Write(ex); }
            return fallback;
        }

        private static string DefaultMessage(string status) => status switch
        {
            ApiUserStatus.Success => "Transaction successful.",
            ApiUserStatus.Failed => "Transaction failed. Amount refunded to your wallet.",
            _ => "Transaction is pending at the operator. Final status will be sent to your callback URL.",
        };
    }
}
