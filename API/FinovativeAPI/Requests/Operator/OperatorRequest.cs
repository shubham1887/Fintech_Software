namespace Project.Requests.Operator
{
    public class OperatorRequest
    {
        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private int _ServiceID;

        public int ServiceID { get => _ServiceID; set => _ServiceID = value; }
    }
}