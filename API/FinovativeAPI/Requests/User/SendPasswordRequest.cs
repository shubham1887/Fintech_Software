namespace Project.Requests.User
{
    public class SendPasswordRequest
    {
        private int _UserID;

        public int UserID { get => _UserID; set => _UserID = value; }
    }
}