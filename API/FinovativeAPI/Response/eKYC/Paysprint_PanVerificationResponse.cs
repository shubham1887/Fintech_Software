using Project.Processing;

namespace Project.Response.eKYC
{
    public class Paysprint_PanVerificationResponse
    {
        private string _FirstName;

        public string FirstName
        {
            get => _FirstName.TrimIfNotNull();
            set => _FirstName = value.TrimIfNotNull();
        }

        private string _MiddleName;

        public string MiddleName
        {
            get => _MiddleName.TrimIfNotNull();
            set => _MiddleName = value.TrimIfNotNull();
        }

        private string _LastName;

        public string LastName
        {
            get => _LastName.TrimIfNotNull();
            set => _LastName = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }

        private string _Category;

        public string Category
        {
            get => _Category.TrimIfNotNull();
            set => _Category = value.TrimIfNotNull();
        }
    }
}