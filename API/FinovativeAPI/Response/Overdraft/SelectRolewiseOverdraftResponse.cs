using Project.Processing;

namespace Project.Response.Overdraft
{
    public class SelectRolewiseOverdraftResponse
    {
        private int _OverdraftMasterID;

        public int OverdraftMasterID
        {
            get => _OverdraftMasterID;
            set => _OverdraftMasterID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private decimal _Overdraft;

        public decimal Overdraft
        {
            get => _Overdraft;
            set => _Overdraft = value;
        }

        private decimal _MaxOverdraft;

        public decimal MaxOverdraft
        {
            get => _MaxOverdraft;
            set => _MaxOverdraft = value;
        }

        private bool _IsActive;
        public bool IsActive { get => _IsActive; set => _IsActive = value; }
    }
}