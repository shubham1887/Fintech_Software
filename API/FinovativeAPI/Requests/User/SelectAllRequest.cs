using Project.Processing;

namespace Project.Requests.User
{
    public class SelectAllRequest
    {
        private int _TakeCount;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }

        private string _Search;

        public string Search
        {
            get => _Search.TrimIfNotNull();
            set => _Search = value.TrimIfNotNull();
        }

        private string _SortColumnName;

        public string SortColumnName
        {
            get => _SortColumnName.TrimIfNotNull();
            set => _SortColumnName = value.TrimIfNotNull();
        }

        private string _SortDirection;

        public string SortDirection
        {
            get => _SortDirection.TrimIfNotNull();
            set => _SortDirection = value.TrimIfNotNull();
        }
        
        private string _NPCIAgentID;

        public string NPCIAgentID
        {
            get => _NPCIAgentID.TrimIfNotNull();
            set => _NPCIAgentID = value.TrimIfNotNull();
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private string _Criteria;

        public string Criteria
        {
            get => _Criteria.TrimIfNotNull();
            set => _Criteria = value.TrimIfNotNull();
        }

        private string _CriteriaText;

        public string CriteriaText
        {
            get => _CriteriaText.TrimIfNotNull();
            set => _CriteriaText = value.TrimIfNotNull();
        }

        private string _CriteriaNumber;

        public string CriteriaNumber
        {
            get => _CriteriaNumber.TrimIfNotNull();
            set => _CriteriaNumber = value.TrimIfNotNull();
        }

        private string _FromDate;

        public string FromDate
        {
            get => _FromDate.TrimIfNotNull();
            set => _FromDate = value.TrimIfNotNull();
        }

        private string _ToDate;

        public string ToDate
        {
            get => _ToDate.TrimIfNotNull();
            set => _ToDate = value.TrimIfNotNull();
        }

        private bool _IsTextValue;

        public bool IsTextValue
        {
            get => _IsTextValue;
            set => _IsTextValue = value;
        }

        private bool _CriteriaValue;

        public bool CriteriaValue
        {
            get => _CriteriaValue;
            set => _CriteriaValue = value;
        }

        private string _ServiceType;
        public string ServiceType
        {
            get => _ServiceType.TrimIfNotNull();
            set => _ServiceType = value.TrimIfNotNull();
        }
    }
}