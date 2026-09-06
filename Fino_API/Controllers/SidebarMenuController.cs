using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class SidebarMenuController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IMenuService _menuService;

        public SidebarMenuController(IConfiguration configuration, IMenuService menuService)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _menuService = menuService;
        }

        // Returns the sidebar menu filtered for the authenticated user's role.
        // SECURITY: the role is taken from the decrypted + session-validated KeyData (the authenticated
        // identity), never from a client-supplied field — so a user cannot request another role's menu.
        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var menu = await _menuService.GetMenu(keyDataModel.UserTypeID, keyDataModel.UserID);
            if (menu.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(new { data = menu }));
        }
    }
}
