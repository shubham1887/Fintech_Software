using MplanCore.Controller;

namespace MplanCore.Response
{
    public class InfoApiDTHPlan
    {
        public string status { get; set; }
        public string message { get; set; }
        public string cust { get; set; }
        public string @operator { get; set; }
        public Data2 data { get; set; }
    }

    public class Data2
    {
        public List<Month> _1month { get; set; }
    }

    public class Month
    {
        private int _amount;

        public int amount
        {
            get => _amount;
            set => _amount = value;
        }

        private string _category;

        public string category
        {
            get => _category;
            set => _category = value;
        }

        private string _validity;

        public string validity
        {
            get => _validity.TrimIfNotNull();
            set => _validity = value.TrimIfNotNull();
        }

        private string _description;

        public string description
        {
            get => _description.TrimIfNotNull();
            set => _description = value.TrimIfNotNull();
        }
    }
}