using Project.Processing;

namespace Project.Response.TicketMaster
{
    public class MasterViewModel
    {
        private int _TicketMasterID;

        public int TicketMasterID
        {
            get => _TicketMasterID;
            set => _TicketMasterID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _Body;

        public string Body
        {
            get => _Body.TrimIfNotNull();
            set => _Body = value.TrimIfNotNull();
        }

        private string _Subject;

        public string Subject
        {
            get => _Subject.TrimIfNotNull();
            set => _Subject = value.TrimIfNotNull();
        }

        private string _Category;

        public string Category
        {
            get => _Category.TrimIfNotNull();
            set => _Category = value.TrimIfNotNull();
        }

        private string _Resolution;

        public string Resolution
        {
            get => _Resolution.TrimIfNotNull();
            set => _Resolution = value.TrimIfNotNull();
        }

        private string _Document;

        public string Document
        {
            get => _Document.TrimIfNotNull();
            set => _Document = value.TrimIfNotNull();
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _ResolutionDate;

        public string ResolutionDate
        {
            get => _ResolutionDate.TrimIfNotNull();
            set => _ResolutionDate = value.TrimIfNotNull();
        }
    }
}