namespace Project.Response.User
{
    public class TargetViewModel
    {
        private decimal _TotalTarget;

        public decimal TotalTarget
        {
            get => _TotalTarget;
            set => _TotalTarget = value;
        }

        private decimal _RemainsTarget;

        public decimal RemainsTarget
        {
            get => _RemainsTarget;
            set => _RemainsTarget = value;
        }
    }
}