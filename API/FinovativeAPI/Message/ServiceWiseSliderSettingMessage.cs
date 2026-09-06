namespace Project.Message
{
    public class ServiceWiseSliderSettingMessage
    {
        public static readonly string UploadSuccess = "Slider uploaded successfully";
        public static readonly string InvalidFileSize = "File size must under 2MB!";
        public static readonly string InvalidFileType = "Only jpg/ jpeg and png files are allowed!";
        public static readonly string InvalidService = "Invalid service";
        public static readonly string InvalidType = "Invalid Type";
        public static readonly string InvalidDocument = "Please select valid slider image";
        public static readonly string DocumentnotUpload = "Invalid slider document";
        public static readonly string NotFound = "Requested slider not found";
        public static readonly string InvalidSlider = "Please select valid slider";
        public static readonly string DeleteSuccess = "Slider deleted successfully";
        public static readonly string DeleteFailure = "Unable to delete slider";
        public static readonly string ActiveSuccess = "Slider activated successfully";
        public static readonly string DeactiveSuccess = "Slider deactivated successfully";
        public static readonly string UpdateFailure = "Unable to update slider";
        public static readonly string UpdateSuccess = "Slider updated successfully";
        public static readonly string DuplicateSlider = "Already exists this slider details";
    }
}