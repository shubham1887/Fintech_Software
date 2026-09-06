using Project.Processing;

namespace Project.Response.PrepaidActivation
{
    public class ViewModel
    {
        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _ActivationType;

        public string ActivationType
        {
            get => _ActivationType.TrimIfNotNull();
            set => _ActivationType = value.TrimIfNotNull();
        }

        private string _AlternateNumber;

        public string AlternateNumber
        {
            get => _AlternateNumber;
            set => _AlternateNumber = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _PrepaidActivationID;

        public int PrepaidActivationID
        {
            get => _PrepaidActivationID;
            set => _PrepaidActivationID = value;
        }

        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull();
            set => _Date = value.TrimIfNotNull();
        }
    }
}