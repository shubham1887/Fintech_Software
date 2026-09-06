namespace Project.Requests.User
{
    public class SelectUserForBulkUpdateRequest
    {
        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }
    }
}