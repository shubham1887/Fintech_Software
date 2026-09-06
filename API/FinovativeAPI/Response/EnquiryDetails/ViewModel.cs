using Project.Processing;

namespace Project.Response.EnquiryDetails
{
    public class ViewModel
    {
        private int _EnquiryDetailsID;

        public int EnquiryDetailsID
        {
            get => _EnquiryDetailsID;
            set => _EnquiryDetailsID = value;
        }

        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.TrimIfNotNull();
            set => _PersonName = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _EnquiryType;

        public string EnquiryType
        {
            get => _EnquiryType.TrimIfNotNull();
            set => _EnquiryType = value.TrimIfNotNull();
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _AdminStatus;

        public string AdminStatus
        {
            get => _AdminStatus.TrimIfNotNull();
            set => _AdminStatus = value.TrimIfNotNull();
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private int dOCInt;

        public int DOCInt
        {
            get => dOCInt;
            set => dOCInt = value;
        }

        private string _AdminRemark;

        public string AdminRemark
        {
            get => _AdminRemark.TrimIfNotNull();
            set => _AdminRemark = value.TrimIfNotNull();
        }
    }
}