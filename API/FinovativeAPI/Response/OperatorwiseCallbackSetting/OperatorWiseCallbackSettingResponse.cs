using Project.Processing;

namespace Project.Response.OperatorwiseCallbackSetting
{
    public class OperatorWiseCallbackSettingResponse
    {
        private int _OperatorID;
        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _Url;
        public string Url
        {
            get => _Url.TrimIfNotNull();
            set => _Url = value.TrimIfNotNull();
        }

        private string _MethodType;
        public string MethodType
        {
            get => _MethodType.TrimIfNotNull();
            set => _MethodType = value.TrimIfNotNull();
        }

        private string _HeaderJson;
        public string HeaderJson
        {
            get => _HeaderJson.TrimIfNotNull();
            set => _HeaderJson = value.TrimIfNotNull();
        }

        private string _DateFormat;
        public string DateFormat
        {
            get => _DateFormat.TrimIfNotNull();
            set => _DateFormat = value.TrimIfNotNull();
        }

        private bool _IsActive;
        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private bool _IsDelete;
        public bool IsDelete
        {
            get => _IsDelete;
            set => _IsDelete = value;
        }

    }
}