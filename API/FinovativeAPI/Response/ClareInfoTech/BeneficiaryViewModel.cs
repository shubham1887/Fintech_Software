namespace Project.Response.ClareInfoTech
{
    public class BeneficiaryViewModel
    {
        private string _id;

        public string id
        {
            get => _id;
            set => _id = value;
        }

        private string _name;

        public string name
        {
            get => _name;
            set => _name = value;
        }

        private string _mobile;

        public string mobile
        {
            get => _mobile;
            set => _mobile = value;
        }

        public string account { get; set; }
        public string bank { get; set; }
        public string ifsc { get; set; }
        public string status { get; set; }
        public string imps { get; set; }
        public string last_success_date { get; set; }
        public string last_success_name { get; set; }
        public string last_success_imps { get; set; }
        public string remitterid { get; set; }
    }
}