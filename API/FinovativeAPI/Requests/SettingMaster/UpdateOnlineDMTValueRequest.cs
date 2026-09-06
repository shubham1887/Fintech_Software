namespace Project.Requests.SettingMaster
{
    public class UpdateOnlineDMTValueRequest
    {
        private bool _IsOnlineDMT;

        public bool IsOnlineDMT
        {
            get => _IsOnlineDMT;
            set => _IsOnlineDMT = value;
        }
    }
}