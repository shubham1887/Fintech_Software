using Project.Processing;

namespace Project.StaticModel
{
    public class CustomFile
    {
        private int _ID;

        public int ID
        {
            get => _ID;
            set => _ID = value;
        }
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Size;

        public string Size
        {
            get => _Size.TrimIfNotNull();
            set => _Size = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _Data;

        public string Data
        {
            get => _Data.TrimIfNotNull();
            set => _Data = value.TrimIfNotNull();
        }
    }
}