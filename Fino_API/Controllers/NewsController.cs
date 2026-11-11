using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.AppNotification;
using FintechApp.Requests.News;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class NewsController : ControllerBase
    {
        private readonly INewsMasterService _service;
        private readonly IAppNotificationService _notifications;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public NewsController(INewsMasterService service, IAppNotificationService notifications, IConfiguration configuration)
        {
            _service = service;
            _notifications = notifications;
            _logger = new Logger();
            string encryptionKey = configuration["Encryption:Key"] ?? string.Empty;
            string encryptionIV = configuration["Encryption:IV"] ?? string.Empty;
            _encryptDecrypt = new EncryptDecrypt(encryptionKey, encryptionIV);
        }

        /// <summary>
        /// Publishing a news item ALSO raises an alert for the whole network.
        ///
        /// Before this, News wrote a row nothing ever read: no screen displayed it, no bell
        /// carried it, no phone was told about it. An operator could publish an offer, see
        /// "Notification published.", and have it reach precisely nobody — which is exactly the
        /// "news daal ke test kiya, kaam nahi karta" report.
        ///
        /// Routing it through the alert pipeline means one publish reaches the bell, the
        /// notifications screen and (where Firebase is configured) the Android tray, and it is
        /// audited in the sent-alert ledger like any other broadcast.
        ///
        /// Best-effort on purpose: the news row is already saved, so a failure here is logged
        /// and never turns a successful publish into an error.
        /// </summary>
        private async Task Announce(KeyDataModel key, string title, string description, string hyperLink, string imagePath)
        {
            try
            {
                await _notifications.Send(new SendAppNotificationRequest
                {
                    Title = title,
                    Body = description,
                    // The picture the operator attached to the announcement. News is how offers
                    // go out, and an offer is mostly the image — without this the alert and the
                    // phone's tray carried the text and silently dropped the artwork.
                    ImageUrl = (imagePath ?? string.Empty).Trim(),
                    // News is how offers and scheme changes are announced, so it lands in the
                    // "Offers" filter rather than sitting among operational alerts.
                    Kind = NotificationKind.Promo,
                    Audience = NotificationAudience.All,
                    // The link the announcement carries, or nothing.
                    //
                    // Deliberately NOT a fallback to "/s/News/Index": that screen is the admin's
                    // news MANAGER and is not in a retailer's menu, so tapping the alert would
                    // land the majority of the network on a permission error. The alert already
                    // carries the whole announcement; with no link there is simply nothing more
                    // to open, and the tap just marks it read.
                    ActionUrl = (hyperLink ?? string.Empty).Trim(),
                }, key);
            }
            catch (Exception ex) { _logger.Write(ex); }
        }

        // News is a platform-wide broadcast: everyone reads it, only the people who run the
        // platform write it. Without this gate any signed-in retailer could publish — or delete —
        // a notification shown to the whole network.
        private static bool CanManage(KeyDataModel k) =>
            k.UserTypeID == StaticModels.UserTypeCode.Admin || k.UserTypeID == StaticModels.UserTypeCode.SupportUser;

        [HttpPost("GetAll")]
        public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                // Unpublished drafts and lapsed announcements are for the people who write them,
                // not for the network: the manager's list keeps both (an expired item is badged,
                // so it can be re-dated or deleted), everyone else reads only what is live now.
                var data = CanManage(keyDataModel)
                    ? await _service.GetAll()
                    : await _service.GetPublished();
                if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("GetById")]
        public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetNewsRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<GetNewsRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var data = await _service.GetByID(request.NewsID);
                if (data == null) return ResponseProcess.GetFailResponse(Messages.NewsMasterMessages.NotFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Create")]
        public async Task<APIResponse> Create([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            CreateNewsRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<CreateNewsRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            if (!CanManage(keyDataModel)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            try
            {
                // The author is whoever is signed in — never whatever the client claims.
                request.UserID = keyDataModel.UserID;
                failureMessage = await _service.Create(request);
                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

                // A draft (IsView off) is not an announcement — only a published item goes out.
                if (request.IsView) await Announce(keyDataModel, request.Title, request.Description, request.HyperLink, request.ImagePath);

                return ResponseProcess.GetSuccessResponse(Messages.NewsMasterMessages.CreatedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Update")]
        public async Task<APIResponse> Update([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            UpdateNewsRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<UpdateNewsRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            if (!CanManage(keyDataModel)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            try
            {
                // Was it already out? Fixing a typo on a published item must NOT re-alert the
                // whole network; flipping a draft to published must.
                bool wasPublished = (await _service.GetByID(request.NewsID))?.IsView ?? false;

                failureMessage = await _service.Update(request);
                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

                if (request.IsView && !wasPublished)
                    await Announce(keyDataModel, request.Title, request.Description, request.HyperLink, request.ImagePath);

                return ResponseProcess.GetSuccessResponse(Messages.NewsMasterMessages.UpdatedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Delete")]
        public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            DeleteNewsRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<DeleteNewsRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            if (!CanManage(keyDataModel)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            try
            {
                failureMessage = await _service.Delete(request.NewsID);
                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(Messages.NewsMasterMessages.DeletedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
