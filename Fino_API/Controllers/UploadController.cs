using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Requests;
using FintechApp.Requests.Upload;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Any authenticated identity may upload — the fine-grained role gate is the central access
    // policy (ApiAccessPolicy grants "upload" to the transacting tier). Restricting the attribute
    // to Admin here blocked retailers/distributors from attaching a payment slip to a Fund Request,
    // which left ReceiptPath empty and the proof thumbnail missing from the Pending Requests queue.
    [JwtAuthorization]
    public class UploadController : ControllerBase
    {
        private static readonly Dictionary<string, string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".webp"] = "image/webp",
            [".gif"] = "image/gif",
        };

        // KYC / verification documents also accept PDF and allow a larger file.
        private static readonly Dictionary<string, string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".webp"] = "image/webp",
            [".pdf"] = "application/pdf",
        };

        private const int MaxFileBytes = 2 * 1024 * 1024; // 2 MB
        private const int MaxDocumentBytes = 5 * 1024 * 1024; // 5 MB

        private readonly IWebHostEnvironment _environment;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public UploadController(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("Image")]
        public async Task<APIResponse> Image([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            UploadImageRequest? request;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<UploadImageRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
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
                string extension = Path.GetExtension(request.FileName ?? string.Empty);
                if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.ContainsKey(extension))
                    return ResponseProcess.GetFailResponse("Only PNG, JPG, WEBP and GIF images are allowed.");

                string base64 = request.FileData ?? string.Empty;
                int commaIndex = base64.IndexOf(',');
                if (base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex > 0)
                    base64 = base64[(commaIndex + 1)..];

                byte[] bytes;
                try { bytes = Convert.FromBase64String(base64); }
                catch { return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); }

                if (bytes.Length == 0)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
                if (bytes.Length > MaxFileBytes)
                    return ResponseProcess.GetFailResponse("Image must be 2 MB or smaller.");

                // Bucket name is sanitised to a single flat folder — no path traversal.
                string folder = new string((request.Folder ?? "misc").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
                if (string.IsNullOrEmpty(folder)) folder = "misc";

                string webRoot = string.IsNullOrEmpty(_environment.WebRootPath)
                    ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                    : _environment.WebRootPath;
                string targetDirectory = Path.Combine(webRoot, "uploads", folder);
                Directory.CreateDirectory(targetDirectory);

                string fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
                await System.IO.File.WriteAllBytesAsync(Path.Combine(targetDirectory, fileName), bytes);

                string publicPath = $"/uploads/{folder}/{fileName}";
                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(new { Path = publicPath }));
                return ResponseProcess.GetSuccessResponse(encryptedResponse);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        /// <summary>Upload a KYC / verification document — accepts images and PDF (up to 5 MB).</summary>
        [HttpPost("Document")]
        public async Task<APIResponse> Document([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            UploadImageRequest? request;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<UploadImageRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
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
                string extension = Path.GetExtension(request.FileName ?? string.Empty);
                if (string.IsNullOrWhiteSpace(extension) || !AllowedDocumentExtensions.ContainsKey(extension))
                    return ResponseProcess.GetFailResponse("Only PNG, JPG, WEBP and PDF documents are allowed.");

                string base64 = request.FileData ?? string.Empty;
                int commaIndex = base64.IndexOf(',');
                if (base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex > 0)
                    base64 = base64[(commaIndex + 1)..];

                byte[] bytes;
                try { bytes = Convert.FromBase64String(base64); }
                catch { return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); }

                if (bytes.Length == 0)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
                if (bytes.Length > MaxDocumentBytes)
                    return ResponseProcess.GetFailResponse("Document must be 5 MB or smaller.");

                string folder = new string((request.Folder ?? "kyc").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
                if (string.IsNullOrEmpty(folder)) folder = "kyc";

                string webRoot = string.IsNullOrEmpty(_environment.WebRootPath)
                    ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                    : _environment.WebRootPath;
                string targetDirectory = Path.Combine(webRoot, "uploads", folder);
                Directory.CreateDirectory(targetDirectory);

                string fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
                await System.IO.File.WriteAllBytesAsync(Path.Combine(targetDirectory, fileName), bytes);

                string publicPath = $"/uploads/{folder}/{fileName}";
                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(new { Path = publicPath }));
                return ResponseProcess.GetSuccessResponse(encryptedResponse);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }
    }
}
