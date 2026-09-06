namespace Project.Requests.TempTransactionRequest
{
    public class SelectRequest
    {
        private int _TakeCount;
        private int _SkipCount;

        public int TakeCount { get => _TakeCount; set => _TakeCount = value; }
        public int SkipCount { get => _SkipCount; set => _SkipCount = value; }
    }
}