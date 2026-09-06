using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.AEPS;
using FintechApp.Responses.AEPS;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class AEPSService : IAEPSService
    {
        private readonly IAEPSSettingRepository _aepsRepo;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly Logger _logger;

        public AEPSService(IAEPSSettingRepository aepsRepo, ITransactionMasterRepository txnRepo)
        {
            _aepsRepo = aepsRepo;
            _txnRepo = txnRepo;
            _logger = new Logger();
        }

        public async Task<(string failureMessage, AEPSSettingResponse? data)> SelectActiveAEPS()
        {
            try
            {
                var all = await _aepsRepo.GetAll();
                var setting = all.Where(a => a.IsActive && !a.IsDeleted)
                                 .OrderBy(a => a.Sequence)
                                 .FirstOrDefault();

                if (setting == null)
                    return ("AEPS setting is not active, please contact to customer care", null);

                return (StatusType.Ok, MapToResponse(setting));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        public async Task<(string failureMessage, List<AEPSSettingResponse> data)> SelectMultiActiveAEPS()
        {
            try
            {
                var all = await _aepsRepo.GetAll();
                var settings = all.Where(a => a.IsActive && !a.IsDeleted)
                                  .OrderBy(a => a.Sequence)
                                  .ToList();

                if (!settings.Any())
                    return ("AEPS setting is not active, please contact to customer care", new());

                return (StatusType.Ok, settings.Select(MapToResponse).ToList());
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, new());
            }
        }

        public async Task<(string failureMessage, AEPSTransactionStatusResponse? data)> GetTransactionStatus(AEPSTransactionStatusRequest request, int userId)
        {
            if (string.IsNullOrWhiteSpace(request.OrderID))
                return (Messages.Common.Message.InvalidRequest, null);

            try
            {
                var transaction = await _txnRepo.GetByProviderRequestID(request.OrderID);

                if (transaction == null)
                    return ("Invalid data", null);

                if (transaction.Status.Equals("PROCESS", StringComparison.OrdinalIgnoreCase))
                    return ("Transaction is still pending", null);

                var response = new AEPSTransactionStatusResponse
                {
                    Status = transaction.Status,
                    Amount = transaction.Amount.ToString("F2"),
                    ReferenceNo = transaction.ReferenceNo,
                    BankName = transaction.DMT_Bank,
                    APIUserRequestID = transaction.APIUserRequestID,
                    Message = $"Transaction {transaction.Status}",
                };

                return (StatusType.Ok, response);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        public PIDOptionResponse SelectPIDOption(SelectPIDOptionRequest request)
        {
            var pid = new PIDOptionResponse { Message = StatusType.Ok };

            string defaultPid = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' posh='UNKNOWN' env='P' /><CustOpts></CustOpts></PidOptions>";
            pid.PID = defaultPid;

            if (request.DeviceType.Equals("com.precision.pb510.rdservice", StringComparison.OrdinalIgnoreCase))
                pid.PID = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' posh='UNKNOWN' env='P' iType='' pType='' otp='' wadh='' /> <Demo></Demo> <CustOpts><Param name='' Stringue='' /></CustOpts></PidOptions>";

            if (request.DeviceType.Equals("com.scl.rdservice", StringComparison.OrdinalIgnoreCase))
                pid.PID = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' posh='UNKNOWN' env='P' iType='' pType='' otp='' wadh='' /> <Demo></Demo> <CustOpts><Param name='PARAM_1' value='' /></CustOpts></PidOptions>";

            if (!request.TransactionType.Equals("Onboarding", StringComparison.OrdinalIgnoreCase))
                return pid;

            // Onboarding PID (with wadh key)
            pid.PID = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' wadh='E0jzJ/P8UopUHAieZn8CKqS4WPMi5ZSYXgfnlfkWjrc=' posh='UNKNOWN' env='P' /><CustOpts></CustOpts></PidOptions>";

            if (request.DeviceType.Equals("com.precision.pb510.rdservice", StringComparison.OrdinalIgnoreCase))
                pid.PID = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' wadh='E0jzJ/P8UopUHAieZn8CKqS4WPMi5ZSYXgfnlfkWjrc=' posh='UNKNOWN' env='P' iType='' pType='' otp='' /> <Demo></Demo> <CustOpts><Param name='' Stringue='' /></CustOpts></PidOptions>";

            if (request.DeviceType.Equals("com.scl.rdservice", StringComparison.OrdinalIgnoreCase))
                pid.PID = "<?xml version='1.0'?><PidOptions ver='1.0'><Opts fCount='1' fType='2' iCount='0' pCount='0' format='0' pidVer='2.0' timeout='10000' wadh='E0jzJ/P8UopUHAieZn8CKqS4WPMi5ZSYXgfnlfkWjrc=' posh='UNKNOWN' env='P' iType='' pType='' otp='' /> <Demo></Demo> <CustOpts><Param name='PARAM_1' value='' /></CustOpts></PidOptions>";

            return pid;
        }

        private static AEPSSettingResponse MapToResponse(AEPSSetting s) => new()
        {
            AEPSSettingID = s.AEPSSettingID,
            Name = s.Name,
            MerchantID = s.MerchantID,
            Token = s.Token,
            Slat = s.Slat,
            PublicKey = s.PublicKey,
            IsActive = s.IsActive,
            IsActiveWeb = s.IsActiveWeb,
            MinimumAmount = s.MinimumAmount,
            MaximumAmount = s.MaximumAmount,
            DisplayName = s.DisplayName,
            Sequence = s.Sequence,
            Mode = s.Mode,
            IsMerchantVerify = s.IsMerchantVerify,
        };
    }
}
