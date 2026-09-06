namespace Project.Response.Quickekyc
{
    public class AadharGenerateOtpAPIResponse
    {
        public Data data { get; set; }

        private string _message;

        public string message
        {
            get => _message;
            set => _message = value;
        }

        private string _status;

        public string status
        {
            get => _status;
            set => _status = value;
        }

        public int request_id { get; set; }

        public class Data
        {
            public bool otp_sent { get; set; }
            public bool if_number { get; set; }
            public bool valid_aadhaar { get; set; }
        }
    }
}