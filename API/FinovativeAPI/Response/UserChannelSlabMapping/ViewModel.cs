using Project.Processing;

namespace Project.Response.UserChannelSlabMapping
{
    public class ViewModel
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _SlabName;

        public string SlabName
        {
            get => _SlabName.TrimIfNotNull();
            set => _SlabName = value.TrimIfNotNull();
        }

        private int _ChannelSlabMasterID;

        public int ChannelSlabMasterID
        {
            get => _ChannelSlabMasterID;
            set => _ChannelSlabMasterID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _UserChannelSlabMappingID;

        public int UserChannelSlabMappingID
        {
            get => _UserChannelSlabMappingID;
            set => _UserChannelSlabMappingID = value;
        }

        private string _Role;

        public string Role
        {
            get => _Role.TrimIfNotNull();
            set => _Role = value.TrimIfNotNull();
        }

        private int _UplineUserID;

        public int UplineUserID
        {
            get => _UplineUserID;
            set => _UplineUserID = value;
        }
    }
}