using Project.Processing;
using System;

namespace Project.Response.LeadGenerationMaster
{
    public class ViewModel
    {

        private int _DOCInt;
        public int DOCInt
        {
            get => _DOCInt;
            set => _DOCInt = value;
        }

        private string _DOC;
        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private int _UserID;
        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _FormID;

        public string FormID
        {
            get => _FormID.TrimIfNotNull();
            set => _FormID = value.TrimIfNotNull();
        }
        
        private int _LeadGenerationMasterID;

        public int LeadGenerationMasterID
        {
            get => _LeadGenerationMasterID;
            set => _LeadGenerationMasterID = value;
        }
        
        private int _LoanTypeMasterID;

        public int LoanTypeMasterID
        {
            get => _LoanTypeMasterID;
            set => _LoanTypeMasterID = value;
        }

        private string _MerchantName;

        public string MerchantName
        {
            get => _MerchantName.TrimIfNotNull();
            set => _MerchantName = value.TrimIfNotNull();
        }

        private string _RetailerName;

        public string RetailerName
        {
            get => _RetailerName.TrimIfNotNull();
            set => _RetailerName = value.TrimIfNotNull();
        }

        private string _ConsumerName;

        public string ConsumerName
        {
            get => _ConsumerName.TrimIfNotNull();
            set => _ConsumerName = value.TrimIfNotNull();
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

        private string _AadharNumber;

        public string AadharNumber
        {
            get => _AadharNumber.TrimIfNotNull();
            set => _AadharNumber = value.TrimIfNotNull();
        }

        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }

        private string _ConsumerType;

        public string ConsumerType
        {
            get => _ConsumerType.TrimIfNotNull();
            set => _ConsumerType = value.TrimIfNotNull();
        }

        private string _RequiredAmount;

        public string RequiredAmount
        {
            get => _RequiredAmount.TrimIfNotNull();
            set => _RequiredAmount = value.TrimIfNotNull();
        }

        private string _LoanType;

        public string LoanType
        {
            get => _LoanType.TrimIfNotNull();
            set => _LoanType = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _ApprovedAmount;

        public string ApprovedAmount
        {
            get => _ApprovedAmount.TrimIfNotNull();
            set => _ApprovedAmount = value.TrimIfNotNull();
        }

        private string _FromDate;

        public string FromDate
        {
            get => _FromDate;
            set => _FromDate = value;
        }

        private string _Gender;

        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }

    }
}