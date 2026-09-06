using Project.Processing;

namespace Project.Response.eKYC
{
    public class eKYCValidateResponse
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

        private string _OutletId;

        public string OutletId
        {
            get => _OutletId.TrimIfNotNull();
            set => _OutletId = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Gender;

        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }

        private string _DateOfBirth;

        public string DateOfBirth
        {
            get => _DateOfBirth.TrimIfNotNull();
            set => _DateOfBirth = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _DistrictName;

        public string DistrictName
        {
            get => _DistrictName.TrimIfNotNull();
            set => _DistrictName = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _ProfilePic;

        public string ProfilePic
        {
            get => _ProfilePic.TrimIfNotNull();
            set => _ProfilePic = value.TrimIfNotNull();
        }
    }
}