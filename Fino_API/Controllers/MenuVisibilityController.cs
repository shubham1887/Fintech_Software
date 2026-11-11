using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Responses;
using FintechApp.Responses.Menu;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Which menu entries this install shows.
    ///
    /// The product ships with every screen it has; an install uses a fraction of them, and the
    /// operator hunting through thirty report rows for the two they use is the complaint this
    /// answers. Hiding is presentation and nothing more — the endpoint behind a hidden screen is
    /// as reachable as it ever was, and <c>ApiAccessPolicy</c> remains the only thing that
    /// decides who may call it.
    ///
    /// Admin and Support both, for reading and for saving. Support is admin-equivalent everywhere
    /// a mistake can be undone, and switching a menu row back on is the definition of reversible —
    /// unlike the tax-document writes next door, which allocate numbers that cannot be withdrawn.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class MenuVisibilityController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IMenuVisibilityService _visibility;

        public MenuVisibilityController(IConfiguration configuration, IMenuVisibilityService visibility)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _visibility = visibility;
        }

        /// <summary>Every menu entry the product has, with its current decision for one scope.</summary>
        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            MenuVisibilityScopeRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<MenuVisibilityScopeRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            // No scope named = the install-wide decisions, which is what the screen opens on.
            int scope = mainRequest?.UserTypeID ?? 0;
            var items = await _visibility.GetCatalog(scope < 0 ? 0 : scope);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(new { data = items, UserTypeID = scope }));
        }

        /// <summary>Records a batch of show/hide decisions.</summary>
        [HttpPost("Save")]
        public async Task<APIResponse> Save([FromForm] APIRequest apiRequest)
        {
            MenuVisibilitySaveRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<MenuVisibilitySaveRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest?.Items == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, changed) = await _visibility.Save(mainRequest.Items, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(new { Changed = changed }));
        }
    }

    public class MenuVisibilityScopeRequest
    {
        /// <summary>Which role's decisions to read; 0 (the default) is the install-wide set.</summary>
        public int UserTypeID { get; set; }
    }

    public class MenuVisibilitySaveRequest
    {
        public List<MenuVisibilityItem> Items { get; set; } = new();
    }
}
