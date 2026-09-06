using Project.Processing;

namespace Project.Response.BBPSConfiguration
{
    public class ViewModel
    {
        private int _BBPSConfigurationID;

        public int BBPSConfigurationID
        {
            get => _BBPSConfigurationID;
            set => _BBPSConfigurationID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _Label;

        public string Label
        {
            get => _Label.TrimIfNotNull();
            set => _Label = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _PlaceHolder;

        public string PlaceHolder
        {
            get => _PlaceHolder.TrimIfNotNull();
            set => _PlaceHolder = value.TrimIfNotNull();
        }

        private string _IsRerquiredStr;

        public string IsRerquiredStr
        {
            get => _IsRerquiredStr.TrimIfNotNull(); set => _IsRerquiredStr = value.TrimIfNotNull();
        }

        private bool _IsRequired;

        public bool IsRequired
        {
            get => _IsRequired;
            set => _IsRequired = value;
        }

        private string _IsDisplayStr;

        public string IsDisplayStr
        {
            get => _IsDisplayStr.TrimIfNotNull(); set => _IsDisplayStr = value.TrimIfNotNull();
        }

        private bool _IsDisplay;

        public bool IsDisplay
        {
            get => _IsDisplay;
            set => _IsDisplay = value;
        }

        private string _ErrorMeeage;

        public string ErrorMeeage
        {
            get => _ErrorMeeage.TrimIfNotNull();
            set => _ErrorMeeage = value.TrimIfNotNull();
        }

        private int _Sequence;

        public int Sequence
        {
            get => _Sequence;
            set => _Sequence = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _ServiceID;
        public int ServiceID { get => _ServiceID; set => _ServiceID = value; }
        private string _ConfigList;

        public string ConfigList
        {
            get => _ConfigList.TrimIfNotNull();
            set => _ConfigList = value.TrimIfNotNull();
        }
    }
}