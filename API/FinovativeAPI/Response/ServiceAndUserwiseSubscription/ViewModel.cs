using Project.Processing;
using System;

namespace Project.Response.ServiceAndUserwiseSubscription
{
    public class ViewModel
    {

        private int _ServiceAndUserwiseSubscriptionID;

        public int ServiceAndUserwiseSubscriptionID
        {
            get => _ServiceAndUserwiseSubscriptionID;
            set => _ServiceAndUserwiseSubscriptionID = value;
        }
        private int _UserServiceSubscriptionChargeID;

        public int UserServiceSubscriptionChargeID
        {
            get => _UserServiceSubscriptionChargeID;
            set => _UserServiceSubscriptionChargeID = value;
        }
        
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }
        private string _EndDate;

        public string EndDate
        {
            get => _EndDate.TrimIfNotNull();
            set => _EndDate = value.TrimIfNotNull();
        }
        private string _PaymentDate;

        public string PaymentDate
        {
            get => _PaymentDate.TrimIfNotNull();
            set => _PaymentDate = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }
        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }



        private string _SubscriptionType;

        public string SubscriptionType
        {
            get => _SubscriptionType.TrimIfNotNull();
            set => _SubscriptionType = value.TrimIfNotNull();
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _PlanName;

        public string PlanName
        {
            get => _PlanName.TrimIfNotNull();
            set => _PlanName = value.TrimIfNotNull();
        }
        private string _LoginID;

        public string LoginID
        {
            get => _LoginID.TrimIfNotNull();
            set => _LoginID = value.TrimIfNotNull();
        }        
        private decimal _OriginalCharge;
        public decimal OriginalCharge
        {
            get => _OriginalCharge;
            set => _OriginalCharge = value;
        }
        private decimal _Charge;
        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }




        public bool IsAutoDebit { get; internal set; }
        public string IsAutoDebitStr
        {
            get => (IsAutoDebit) ? "Yes" : "No";
        }
        public bool IsAdminApproved { get; internal set; }

        public string IsAdminApprovedStr
        {
            get => (IsAdminApproved) ? "Yes" : "No";
        }

        public DateTime StartDate { get; internal set; }
        public int UserTypeID { get; internal set; }



        private int _ServiceSubscriptionPlanMasterID;

        public int ServiceSubscriptionPlanMasterID
        {
            get => _ServiceSubscriptionPlanMasterID;
            set => _ServiceSubscriptionPlanMasterID = value;
        }

        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }
       
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private int _DayCount;

        public int DayCount
        {
            get => _DayCount;
            set => _DayCount = value;
        }
    }
}
