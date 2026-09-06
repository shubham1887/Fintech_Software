using Project.Processing;

namespace Project.Response.KYCApprove
{
    public class ViewModel
    {
        private int _ApproveUserID;

        public int ApproveUserID
        {
            get => _ApproveUserID;
            set => _ApproveUserID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _Remark;
        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }


    }
}