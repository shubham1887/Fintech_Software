using Project.Processing;


namespace Project.Requests.UserWisePlanLimitSetting
{
    public class AllActiveDeactiveRequest
    {
        private bool _IsActive;
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

    }
}