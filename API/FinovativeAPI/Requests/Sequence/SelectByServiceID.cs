namespace Project.Requests.Sequence
{
    public class SelectByServiceID
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
    }
}