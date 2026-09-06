using Project.Processing;

namespace Project.Response.eKYC
{
    public class Cashfree_eKYCValidateResponse
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

        private string _AadharName;

        public string AadharName
        {
            get => _AadharName.TrimIfNotNull();
            set => _AadharName = value.TrimIfNotNull();
        }

        private string _PanName;

        public string PanName
        {
            get => _PanName.TrimIfNotNull();
            set => _PanName = value.TrimIfNotNull();
        }

        private string _PanCategory;

        public string PanCategory
        {
            get => _PanCategory.TrimIfNotNull();
            set => _PanCategory = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _Gender;

        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }

        private string _Dob;

        public string Dob
        {
            get => _Dob.TrimIfNotNull();
            set => _Dob = value.TrimIfNotNull();
        }

        private string _AadhaarNumber;

        public string AadhaarNumber
        {
            get => _AadhaarNumber.TrimIfNotNull();
            set => _AadhaarNumber = value.TrimIfNotNull();
        }

        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }

        private string _Pin;

        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        private string _ProfileImage;

        public string ProfileImage
        {
            get => _ProfileImage.TrimIfNotNull();
            set => _ProfileImage = value.TrimIfNotNull();
        }

        private string _FullName;

        public string FullName
        {
            get => _FullName.TrimIfNotNull();
            set => _FullName = value.TrimIfNotNull();
        }
    }
}
