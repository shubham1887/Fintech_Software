using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class UserWiseDMTRangeViewModel
    {

        private string _UserName;
        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private int _Range1_2;
        public int Range1_2
        {
            get => _Range1_2;
            set => _Range1_2 = value;
        }
        private int _Range3_1000;
        public int Range3_1000
        {
            get => _Range3_1000;
            set => _Range3_1000 = value;
        }
        private int _Range1001_2000;
        public int Range1001_2000
        {
            get => _Range1001_2000;
            set => _Range1001_2000 = value;
        }
        private int _Range2001_3000;
        public int Range2001_3000
        {
            get => _Range2001_3000;
            set => _Range2001_3000 = value;
        }
        private int _Range3001_4000;
        public int Range3001_4000
        {
            get => _Range3001_4000;
            set => _Range3001_4000 = value;
        }
        private int _Range4001_5000;
        public int Range4001_5000
        {
            get => _Range4001_5000;
            set => _Range4001_5000 = value;
        }
        private int _Range5001_10000;
        public int Range5001_10000
        {
            get => _Range5001_10000;
            set => _Range5001_10000 = value;
        }
        private int _Range10001_15000;
        public int Range10001_15000
        {
            get => _Range10001_15000;
            set => _Range10001_15000 = value;
        }
        private int _Range15001_20000;
        public int Range15001_20000
        {
            get => _Range15001_20000;
            set => _Range15001_20000 = value;
        }
        private int _Range20001_25000;
        public int Range20001_25000
        {
            get => _Range20001_25000;
            set => _Range20001_25000 = value;
        }
        private int _Range25001_50000;
        public int Range25001_50000
        {
            get => _Range25001_50000;
            set => _Range25001_50000 = value;
        }
        private int _Range50001_1L;
        public int Range50001_1L
        {
            get => _Range50001_1L;
            set => _Range50001_1L = value;
        }
        private int _TotalTransactions;
        public int TotalTransactions
        {
            get => _TotalTransactions;
            set => _TotalTransactions = value;
        }
        

    }
}
