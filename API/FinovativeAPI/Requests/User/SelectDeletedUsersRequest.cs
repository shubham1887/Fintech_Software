namespace Project.Requests.User
{
    public class SelectDeletedUsersRequest
    {
        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }
    }
}