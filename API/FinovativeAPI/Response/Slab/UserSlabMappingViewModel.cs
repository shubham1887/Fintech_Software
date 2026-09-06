using Project.Processing;

namespace Project.Response.Slab
{
    public class UserSlabMappingViewModel
    {
        private string _FirmName;

        private string _SlabName;
        private int _SlabMasterID;

        private int _UserID;

        private int _UserSlabMappingID;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        public string SlabName
        {
            get => _SlabName.TrimIfNotNull();
            set => _SlabName = value.TrimIfNotNull();
        }

        public int SlabID
        {
            get => _SlabMasterID;
            set => _SlabMasterID = value;
        }

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        public int UserSlabMappingID
        {
            get => _UserSlabMappingID;
            set => _UserSlabMappingID = value;
        }

        private string _Role;
        public string Role { get => _Role.TrimIfNotNull(); set => _Role = value.TrimIfNotNull(); }
    }
}