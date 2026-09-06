using Project.Processing;

namespace Project.Response.Channel
{
    public class MasterViewModel
    {
        private string _Name;
        private int _ChannelSlabMasterID;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        public int ChannelSlabMasterID
        {
            get => _ChannelSlabMasterID;
            set => _ChannelSlabMasterID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private int _UplineUserID;

        public int UplineUserID
        {
            get => _UplineUserID;
            set => _UplineUserID = value;
        }
    }
}