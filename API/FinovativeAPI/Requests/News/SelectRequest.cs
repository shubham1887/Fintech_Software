namespace Project.Requests.News
{
    public class SelectRequest
    {
        private int _NewsID;

        public int NewsID
        {
            get => _NewsID;
            set => _NewsID = value;
        }

        private int _TakeCount;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }
    }
}