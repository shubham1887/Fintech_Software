using Project.Processing;

namespace Project.Response.eKYC
{
    public class PDRS_eKYCValidateAPIResponse
    {
        public Result result { get; set; }

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

        public class Address
        {
            private string _dist;

            public string dist
            {
                get => _dist.TrimIfNotNull();
                set => _dist = value.TrimIfNotNull();
            }

            private string _state;

            public string state
            {
                get => _state.TrimIfNotNull();
                set => _state = value.TrimIfNotNull();
            }

            private string _vtc;

            public string vtc
            {
                get => _vtc.TrimIfNotNull();
                set => _vtc = value.TrimIfNotNull();
            }

            private string _subdist;

            public string subdist
            {
                get => _subdist.TrimIfNotNull();
                set => _subdist = value.TrimIfNotNull();
            }

            private string _street;

            public string street
            {
                get => _street.TrimIfNotNull();
                set => _street = value.TrimIfNotNull();
            }

            private string _house;

            public string house
            {
                get => _house.TrimIfNotNull();
                set => _house = value.TrimIfNotNull();
            }

            private string _landmark;

            public string landmark
            {
                get => _landmark.TrimIfNotNull();
                set => _landmark = value.TrimIfNotNull();
            }

            private string _loc;

            public string loc
            {
                get => _loc.TrimIfNotNull();
                set => _loc = value.TrimIfNotNull();
            }

            private string _zip;

            public string zip
            {
                get => _zip.TrimIfNotNull();
                set => _zip = value.TrimIfNotNull();
            }
        }

        public class Data
        {
            private string _full_name;

            public string full_name
            {
                get => _full_name.TrimIfNotNull();
                set => _full_name = value.TrimIfNotNull();
            }

            private string _aadhaar_number;

            public string aadhaar_number
            {
                get => _aadhaar_number.TrimIfNotNull();
                set => _aadhaar_number = value.TrimIfNotNull();
            }

            private string _dob;

            public string dob
            {
                get => _dob.TrimIfNotNull();
                set => _dob = value.TrimIfNotNull();
            }

            private string _gender;

            public string gender
            {
                get => _gender.TrimIfNotNull();
                set => _gender = value.TrimIfNotNull();
            }

            public Address address { get; set; }

            private string _profile_image;

            public string profile_image
            {
                get => _profile_image.TrimIfNotNull();
                set => _profile_image = value.TrimIfNotNull();
            }
        }

        public class Result
        {
            public Data data { get; set; }
            public int status_code { get; set; }
            public bool success { get; set; }
            public string message_code { get; set; }
        }
    }
}