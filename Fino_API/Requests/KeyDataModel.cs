using FintechApp.Helpers;
using FintechApp.Messages;
using FintechApp.StaticModels;

namespace FintechApp.Requests
{
    public class KeyDataModel
    {
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
        public string SessionToken { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty; // Web, Android, API
        public string IPAddress { get; set; } = string.Empty;

        public string IsValidModalState()
        {
            if (UserID <= 0)
                return Messages.Common.Message.InvalidData;

            if (UserTypeID <= 0)
                return Messages.Common.Message.InvalidData;

            if (SessionToken.IsNullOrWhiteSpace())
                return Messages.Common.Message.InvalidSession;

            return StatusType.Ok;
        }
    }
}
