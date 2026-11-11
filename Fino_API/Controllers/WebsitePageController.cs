using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.WebsitePage;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Public-website page builder. Admin manages pages (encrypted, admin-only);
    /// the anonymous <see cref="Public"/> endpoint serves published pages to the
    /// public site without a login (plain JSON — there is no session to encrypt for).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization(UserTypeCode.Admin)]
    public class WebsitePageController : ControllerBase
    {
        private readonly IWebsitePageService _service;
        private readonly IMemoryCache _cache;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public WebsitePageController(IWebsitePageService service, IMemoryCache cache, IConfiguration configuration)
        {
            _service = service;
            _cache = cache;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        /// <summary>
        /// Published pages for the public website — anonymous, plain JSON.
        ///
        /// This single call gates the paint of every public page (/, /services, /about, /contact):
        /// the SPA has nothing to render until it answers, so whatever it costs is what a first-time
        /// visitor experiences as "the site is slow". Two things keep it off the database:
        ///
        ///   • a short in-process cache, because the content changes when an operator edits a page
        ///     in the Website Builder — a handful of times a year — and is read by every visitor;
        ///   • a public Cache-Control, so a repeat visit and any CDN in front of the API answer
        ///     without reaching us at all.
        ///
        /// The window is deliberately short enough that an operator who publishes an edit sees it
        /// on a refresh rather than wondering whether it saved. Nothing here is per-user: the
        /// endpoint is anonymous and the payload is identical for everyone, which is exactly what
        /// makes a shared cache safe.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("Public")]
        public async Task<APIResponse> Public()
        {
            try
            {
                var pages = await _cache.GetOrCreateAsync(PublicPagesCacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = PublicPagesCacheWindow;
                    return await _service.GetPublished();
                });

                Response.Headers.CacheControl = $"public, max-age={(int)PublicPagesCacheWindow.TotalSeconds}";
                return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(pages ?? new()));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        /// <summary>Cache key for the anonymous published-pages payload (one entry, not per-user).</summary>
        private const string PublicPagesCacheKey = "website-pages:public";

        /// <summary>
        /// How long the published-pages payload is reused, in process and in the browser.
        ///
        /// Marketing copy, not a balance: two minutes of staleness is invisible to a visitor and
        /// takes the public site off the database almost entirely. An operator publishing an edit
        /// does not have to wait for it — <see cref="InvalidatePublicPages"/> is called from every
        /// write below.
        /// </summary>
        private static readonly TimeSpan PublicPagesCacheWindow = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Drops the cached payload so an edit is live on the next request.
        ///
        /// Without this the two-minute window would apply to the operator too, and "I saved it and
        /// the site still shows the old text" reads as the Website Builder being broken.
        /// </summary>
        private void InvalidatePublicPages() => _cache.Remove(PublicPagesCacheKey);

        [HttpPost("GetAll")]
        public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var pages = await _service.GetAll();
                if (pages == null || pages.Count == 0)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(pages)));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("GetById")]
        public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetWebsitePageRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<GetWebsitePageRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || request == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var page = await _service.GetByID(request.WebsitePageMasterID);
                if (page == null)
                    return ResponseProcess.GetFailResponse(Messages.WebsitePageMessages.NotFound);

                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(page)));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("Create")]
        public async Task<APIResponse> Create([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            CreateWebsitePageRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<CreateWebsitePageRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || request == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _service.Create(request);
                if (failureMessage.EqualsIgnoreCase(StatusType.Ok)) InvalidatePublicPages();
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.WebsitePageMessages.CreatedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("Update")]
        public async Task<APIResponse> Update([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            UpdateWebsitePageRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<UpdateWebsitePageRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || request == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _service.Update(request);
                if (failureMessage.EqualsIgnoreCase(StatusType.Ok)) InvalidatePublicPages();
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.WebsitePageMessages.UpdatedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("Delete")]
        public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            DeleteWebsitePageRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<DeleteWebsitePageRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || request == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _service.Delete(request.WebsitePageMasterID);
                if (failureMessage.EqualsIgnoreCase(StatusType.Ok)) InvalidatePublicPages();
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.WebsitePageMessages.DeletedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }
    }
}
