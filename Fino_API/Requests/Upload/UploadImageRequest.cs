namespace FintechApp.Requests.Upload
{
    public class UploadImageRequest
    {
        public string FileName { get; set; } = string.Empty;

        /// <summary>Base64 file content — plain base64 or a data-URI ("data:image/png;base64,....").</summary>
        public string FileData { get; set; } = string.Empty;

        /// <summary>Target bucket under /uploads — e.g. "services", "operators".</summary>
        public string Folder { get; set; } = "misc";
    }
}
