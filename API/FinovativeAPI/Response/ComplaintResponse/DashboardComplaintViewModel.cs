namespace Project.Response.ComplaintResponse
{
    public class DashboardComplaintViewModel
    {
        private int _ComplaintCount;

        public int ComplaintCount
        {
            get => _ComplaintCount;
            set => _ComplaintCount = value;
        }

        private decimal _ComplaintAmount;

        public decimal ComplaintAmount
        {
            get => _ComplaintAmount;
            set => _ComplaintAmount = value;
        }
    }
}