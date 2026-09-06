
using Project.Processing;
namespace Project.Response.eKYC
{
    public class GSTViewResponse
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _GstStatus;

        public string GstStatus
        {
            get => _GstStatus.TrimIfNotNull();
            set => _GstStatus = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _CompanyName;

        public string CompanyName
        {
            get => _CompanyName.TrimIfNotNull();
            set => _CompanyName = value.TrimIfNotNull();
        }

        private string _CompanyType;

        public string CompanyType
        {
            get => _CompanyType.TrimIfNotNull();
            set => _CompanyType = value.TrimIfNotNull();
        }

        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }

        private string _GSTNumber;

        public string GSTNumber
        {
            get => _GSTNumber.TrimIfNotNull();
            set => _GSTNumber = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _DateOfRegistration;

        public string DateOfRegistration
        {
            get => _DateOfRegistration.TrimIfNotNull();
            set => _DateOfRegistration = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _TaxPayerType;

        public string TaxPayerType
        {
            get => _TaxPayerType.TrimIfNotNull();
            set => _TaxPayerType = value.TrimIfNotNull();
        }
    }
}
