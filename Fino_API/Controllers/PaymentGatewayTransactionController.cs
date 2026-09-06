using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.PaymentGateway;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class PaymentGatewayTransactionController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IPaymentGatewayService _pgService;

        public PaymentGatewayTransactionController(IConfiguration configuration, IPaymentGatewayService pgService)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _pgService = pgService;
        }

        // Payment gateway initiation — requires external PG provider integration (pending)
        [HttpPost("Initiate")]
        public async Task<APIResponse> Initiate([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse("Payment gateway Initiate pending: external PG provider integration not yet implemented."));
        }

        // Lookup payment gateway transaction by OrderID
        [HttpPost("SelectByOrderID")]
        public async Task<APIResponse> SelectByOrderID([FromForm] APIRequest apiRequest)
        {
            SelectPGByOrderRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<SelectPGByOrderRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var all = await _pgService.GetAll();
            var record = all?.FirstOrDefault(p => p.OrderID == req.OrderID && p.UserID == keyDataModel.UserID);
            if (record == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(record));
        }

        // Receipt view — same as SelectByOrderID but includes user context for receipt display
        [HttpPost("SelectForReceipt")]
        public async Task<APIResponse> SelectForReceipt([FromForm] APIRequest apiRequest)
        {
            SelectPGByOrderRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<SelectPGByOrderRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var all = await _pgService.GetAll();
            var record = all?.FirstOrDefault(p => p.OrderID == req.OrderID);
            if (record == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(record));
        }

        // Transaction status check — requires live PG provider call (pending)
        [HttpPost("TransactionStatus")]
        public async Task<APIResponse> TransactionStatus([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse("TransactionStatus pending: live PG provider status check not yet implemented."));
        }

        // Paginated payment gateway transaction report for current user
        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            SelectPGReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<SelectPGReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var all = await _pgService.GetByUserID(keyDataModel.UserID);
            if (all == null || all.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);

            DateTime.TryParse(req.FromDate, out DateTime from);
            DateTime.TryParse(req.ToDate, out DateTime to);
            if (to == default) to = DateTime.Now;
            if (from == default) from = to.AddDays(-30);

            var filtered = all.Where(p => p.DateTime >= from && p.DateTime <= to)
                              .OrderByDescending(p => p.DateTime)
                              .Skip((req.PageNumber - 1) * req.PageSize)
                              .Take(req.PageSize)
                              .ToList();

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(new { Count = filtered.Count, data = filtered }));
        }

        // Same as Select — named SelectReport for API compatibility
        [HttpPost("SelectReport")]
        public async Task<APIResponse> SelectReport([FromForm] APIRequest apiRequest)
        {
            return await Select(apiRequest);
        }

        // Returns the active payment gateway record for the current user
        [HttpPost("SelectActivePaymentGateway")]
        public async Task<APIResponse> SelectActivePaymentGateway([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var all = await _pgService.GetByUserID(keyDataModel.UserID);
            var active = all?.Where(p => p.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
                             .OrderByDescending(p => p.DateTime)
                             .FirstOrDefault();
            if (active == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(active));
        }

        // Returns all active payment gateway records for the current user (V2 — list)
        [HttpPost("SelectActivePaymentGatewayV2")]
        public async Task<APIResponse> SelectActivePaymentGatewayV2([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var all = await _pgService.GetByUserID(keyDataModel.UserID);
            var active = all?.Where(p => p.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
                             .OrderByDescending(p => p.DateTime)
                             .ToList();
            if (active == null || active.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(active));
        }

        // Static QR code details — requires QR generation service (pending)
        [HttpPost("SelectStaticQR")]
        public async Task<APIResponse> SelectStaticQR([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse("SelectStaticQR pending: QR code generation service not yet implemented."));
        }

        // Update UPI open status — requires UPI provider integration (pending)
        [HttpPost("UpdateOpenUPIStatus")]
        public async Task<APIResponse> UpdateOpenUPIStatus([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse("UpdateOpenUPIStatus pending: UPI provider integration not yet implemented."));
        }

        // Get payment gateway transaction by PaymentGatewayID
        [HttpPost("SelectByID")]
        public async Task<APIResponse> SelectByID([FromForm] APIRequest apiRequest)
        {
            GetPaymentGatewayRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<GetPaymentGatewayRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var data = await _pgService.GetByID(req.PaymentGatewayID);
            if (data == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Manual status update for payment gateway transaction — requires audit + status update service (pending)
        [HttpPost("UpdateStatusManually")]
        public async Task<APIResponse> UpdateStatusManually([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse("UpdateStatusManually pending: PG status update with audit log not yet implemented."));
        }

        // Detailed transaction view for a specific PG transaction
        [HttpPost("SelectTransactionDetail")]
        public async Task<APIResponse> SelectTransactionDetail([FromForm] APIRequest apiRequest)
        {
            GetPaymentGatewayRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<GetPaymentGatewayRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var data = await _pgService.GetByID(req.PaymentGatewayID);
            if (data == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Commission view by role for payment gateway — requires commission service (pending)
        [HttpPost("SelectRoleWithCommission")]
        public async Task<APIResponse> SelectRoleWithCommission([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse("SelectRoleWithCommission pending: role-wise commission aggregation not yet implemented."));
        }

        // Last 10 payment gateway deposits for the current user
        [HttpPost("SelectLast10PaymentGateway")]
        public async Task<APIResponse> SelectLast10PaymentGateway([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var all = await _pgService.GetByUserID(keyDataModel.UserID);
            var last10 = all?.OrderByDescending(p => p.DateTime).Take(10).ToList();
            if (last10 == null || last10.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(last10));
        }

        // Last 10 credit-to-bank transactions (ServiceID=10 in old system)
        [HttpPost("SelectLast10CreditToBank")]
        public async Task<APIResponse> SelectLast10CreditToBank([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var all = await _pgService.GetByUserID(keyDataModel.UserID);
            // CreditToBank service differentiated by PayMode=CreditCard or matching ServiceID
            var creditToBank = all?.Where(p => p.PayMode.Equals("CreditCard", StringComparison.OrdinalIgnoreCase)
                                               || p.PayMode.Equals("CREDITTOBANK", StringComparison.OrdinalIgnoreCase))
                                  .OrderByDescending(p => p.DateTime)
                                  .Take(10)
                                  .ToList();
            if (creditToBank == null || creditToBank.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(creditToBank));
        }

        // Webhook endpoint for payment gateway provider callbacks (pending: signature verification + status update)
        [HttpPost("Webhook")]
        public async Task<APIResponse> Webhook([FromForm] APIRequest apiRequest)
        {
            return await Task.FromResult(ResponseProcess.GetFailResponse("Webhook pending: PG provider signature verification and callback processing not yet implemented."));
        }
    }
}
