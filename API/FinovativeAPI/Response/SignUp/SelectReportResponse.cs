using project.StaticModel;
using Project.Processing;

namespace Project.Response.SignUp
{
    public class SelectReportResponse
    {
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }


        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _PanNo;

        public string PanNo
        {
            get => _PanNo.TrimIfNotNull();
            set => _PanNo = value.TrimIfNotNull();
        }

        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }

        private string _ProfileID;

        public string ProfileID
        {
            get => _ProfileID.TrimIfNotNull();
            set => _ProfileID = value.TrimIfNotNull();
        }

        private string _RedirectURL;

        public string RedirectURL
        {
            get => _RedirectURL.TrimIfNotNull();
            set => _RedirectURL = value.TrimIfNotNull();
        }

        private string _ReferralCode;

        public string ReferralCode
        {
            get => _ReferralCode.TrimIfNotNull();
            set => _ReferralCode = value.TrimIfNotNull();
        }

        private string _EmployeeID;

        public string EmployeeID
        {
            get => _EmployeeID.TrimIfNotNull();
            set => _EmployeeID = value.TrimIfNotNull();
        }
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }


    }
}