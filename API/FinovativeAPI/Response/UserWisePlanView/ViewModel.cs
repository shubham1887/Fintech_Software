using Project.Processing;

namespace Project.Response.UserWisePlanView
{
    public class ViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _OperatorID;
        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _ConsumerNumber;
        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
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

        private string _DOC;
        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private int _DOCInt;
        public int DOCInt
        {
            get => _DOCInt;
            set => _DOCInt = value;
        }

        private int _ServiceID;
        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _MobileServiceCount;
        public int MobileServiceCount
        {
            get => _MobileServiceCount;
            set => _MobileServiceCount = value;
        }
        
        private int _DTHServiceCount;
        public int DTHServiceCount
        {
            get => _DTHServiceCount;
            set => _DTHServiceCount = value;
        }

    }
}
