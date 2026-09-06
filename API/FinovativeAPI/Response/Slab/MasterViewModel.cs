using Project.Processing;

namespace Project.Response.Slab
{
    public class MasterViewModel
    {
        private string _Name;
        private int _SlabID;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

        public int SlabID
        {
            get => _SlabID;
            set => _SlabID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }
    }
}