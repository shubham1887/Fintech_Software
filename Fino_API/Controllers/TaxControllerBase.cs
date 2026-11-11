using FintechApp.Helpers;
using FintechApp.Requests;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Shared plumbing for the tax and invoicing controllers.
    ///
    /// Every endpoint in this application repeats the same twelve lines: decrypt KeyData, decrypt
    /// Request, null-check both, validate the model state, call the service, map a failure message,
    /// encrypt the result, and wrap the lot in two try/catch blocks. Across the eight controllers
    /// this subsystem adds that would be some thirty copies, and a copy that drifts is a copy that
    /// fails differently from its neighbours — which is exactly the kind of difference nobody finds
    /// until a role gets a blank screen.
    ///
    /// So it lives here once. The endpoints below are then short enough to read as what they
    /// actually do.
    /// </summary>
    [ApiController]
    public abstract class TaxControllerBase : ControllerBase
    {
        protected readonly EncryptDecrypt Crypto;
        protected readonly Logger Log;

        protected TaxControllerBase(IConfiguration configuration)
        {
            Log = new Logger();
            Crypto = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        /// <summary>An endpoint that needs only the caller's identity.</summary>
        protected async Task<APIResponse> Handle(
            APIRequest apiRequest, Func<KeyDataModel, Task<(string Failure, object? Data)>> work)
        {
            KeyDataModel? keyData;
            try
            {
                keyData = JsonConvert.DeserializeObject<KeyDataModel>(Crypto.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { Log.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyData == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string state = keyData.IsValidModalState();
            if (!state.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(state);

            try
            {
                var (failure, data) = await work(keyData);
                if (!string.IsNullOrWhiteSpace(failure)) return ResponseProcess.GetFailResponse(failure);
                return ResponseProcess.GetSuccessResponse(Crypto.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { Log.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>An endpoint that also carries a request body.</summary>
        protected async Task<APIResponse> Handle<TRequest>(
            APIRequest apiRequest, Func<TRequest, KeyDataModel, Task<(string Failure, object? Data)>> work)
            where TRequest : class
        {
            TRequest? request;
            KeyDataModel? keyData;
            try
            {
                request = JsonConvert.DeserializeObject<TRequest>(Crypto.Decrypt(apiRequest.Request));
                keyData = JsonConvert.DeserializeObject<KeyDataModel>(Crypto.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { Log.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (request == null || keyData == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string state = keyData.IsValidModalState();
            if (!state.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(state);

            try
            {
                var (failure, data) = await work(request, keyData);
                if (!string.IsNullOrWhiteSpace(failure)) return ResponseProcess.GetFailResponse(failure);
                return ResponseProcess.GetSuccessResponse(Crypto.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { Log.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>Succeeded, with a payload. Deliberately hides ControllerBase.Ok: every action
        /// here returns the encrypted envelope, never a bare 200, and shadowing the inherited name
        /// means a reflex `return Ok(x)` lands in the envelope instead of bypassing it.</summary>
        protected static new (string Failure, object? Data) Ok(object? data) => (string.Empty, data);

        /// <summary>Refused, with a reason the caller can act on.</summary>
        protected static (string Failure, object? Data) Refuse(string why) => (why, null);
    }
}
