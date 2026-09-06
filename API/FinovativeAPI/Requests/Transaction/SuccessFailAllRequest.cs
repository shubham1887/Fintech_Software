namespace Project.Requests.Transaction
{
    public class SuccessFailAllRequest
    {
        private int _ID;

        public int ID
        {
            get => _ID;
            set => _ID = value;
        }
        private int _Pin;

        public int Pin
        {
            get => _Pin;
            set => _Pin = value;
        }
    }
}