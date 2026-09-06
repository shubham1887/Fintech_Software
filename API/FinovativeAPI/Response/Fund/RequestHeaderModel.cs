namespace Project.Response.Fund
{
    public class RequestHeaderModel
    {
        private decimal _TotalSuccess;

        public decimal TotalSuccess
        {
            get => _TotalSuccess;
            set => _TotalSuccess = value;
        }

        private decimal _TotalFailure;

        public decimal TotalFailure
        {
            get => _TotalFailure;
            set => _TotalFailure = value;
        }

        private decimal _TotalProcess;

        public decimal TotalProcess
        {
            get => _TotalProcess;
            set => _TotalProcess = value;
        }
    }
}