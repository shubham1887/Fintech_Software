using Project.Processing;

namespace Project.Response.AEPS
{
    public class FingerPrintDeviceResponse
    {
        private int _FingerPrintDeviceTypeID;

        public int FingerPrintDeviceTypeID
        {
            get => _FingerPrintDeviceTypeID;
            set => _FingerPrintDeviceTypeID = value;
        }

        private string _DeviceName;
        public string DeviceName
        {
            get => _DeviceName.TrimIfNotNull();
            set => _DeviceName = value.TrimIfNotNull();
        }

        private string _Value;
        public string Value
        {
            get => _Value.TrimIfNotNull();
            set => _Value = value.TrimIfNotNull();
        }
    }
}