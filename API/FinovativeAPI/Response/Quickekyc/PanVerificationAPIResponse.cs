using Project.Processing;

namespace Project.Response.Quickekyc
{
    public class PanVerificationAPIResponse
    {
        private string _message;

        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }

        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        public Data data { get; set; }

        public class Data
        {
            private string _pan_number;

            public string pan_number
            {
                get => _pan_number.TrimIfNotNull();
                set => _pan_number = value.TrimIfNotNull();
            }

            private string _full_name;

            public string full_name
            {
                get => _full_name.TrimIfNotNull();
                set => _full_name = value.TrimIfNotNull();
            }

            private string _category;

            public string category
            {
                get => _category.TrimIfNotNull();
                set => _category = value.TrimIfNotNull();
            }
        }
    }
}