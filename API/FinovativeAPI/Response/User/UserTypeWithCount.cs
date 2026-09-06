using Project.Processing;

namespace Project.Response.User
{
    public class UserTypeWithCount
    {
        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;

            set => _UserTypeID = value;
        }

        private int _TotalCount;

        public int TotalCount
        {
            get => _TotalCount;

            set => _TotalCount = value;
        }

        private int _Count;

        public int Count
        {
            get => _Count;

            set => _Count = value;
        }
    }
}