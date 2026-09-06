namespace Project.Requests.KYC
{
    public class ApproveRequest
    {
        private int _UserID;
        private bool _IsApproved;
        private string _Remark;

        private int _UserDocumentID;

        public int UserID { get => _UserID; set => _UserID = value; }
        public bool IsApproved { get => _IsApproved; set => _IsApproved = value; }
        public string Remark { get => _Remark; set => _Remark = value; }
        public int UserDocumentID { get => _UserDocumentID; set => _UserDocumentID = value; }
    }
}