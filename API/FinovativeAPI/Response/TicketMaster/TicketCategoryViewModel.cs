using Project.Processing;

namespace Project.Response.TicketMaster
{
    public class TicketCategoryViewModel
    {
        private int _TicketCategoryID;

        public int TicketCategoryID
        {
            get => _TicketCategoryID;
            set => _TicketCategoryID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }
    }
}