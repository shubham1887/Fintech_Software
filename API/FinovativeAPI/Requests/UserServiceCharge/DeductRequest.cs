namespace Project.Requests.UserServiceCharge
{
    public class DeductRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
    }
}