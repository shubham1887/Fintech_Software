using Project.Processing;

namespace Project.Response.EaseBuzz
{
    public class EaseBuzzCallbackResponse
    {
        public Data data = new Data();
    }

    public class Data
    {
        private string _id;
        public string id
        {
            get
            {
                return _id.TrimIfNotNull();
            }
            set
            {
                _id = value.TrimIfNotNull();
            }
        }

        private string _remitter_full_name;
        public string remitter_full_name
        {
            get
            {
                return _remitter_full_name.TrimIfNotNull();
            }
            set
            {
                _remitter_full_name = value.TrimIfNotNull();
            }
        }

        private string _remitter_account_number;
        public string remitter_account_number
        {
            get
            {
                return _remitter_account_number.TrimIfNotNull();
            }
            set
            {
                _remitter_account_number = value.TrimIfNotNull();
            }
        }

        private string _unique_transaction_reference;
        public string unique_transaction_reference
        {
            get
            {
                return _unique_transaction_reference.TrimIfNotNull();
            }
            set
            {
                _unique_transaction_reference = value.TrimIfNotNull();
            }
        }

        private string _status;
        public string status
        {
            get
            {
                return _status.TrimIfNotNull();
            }
            set
            {
                _status = value.TrimIfNotNull();
            }
        }

        private string _Authorization;
        public string Authorization
        {
            get
            {
                return _Authorization.TrimIfNotNull();
            }
            set
            {
                _Authorization = value.TrimIfNotNull();
            }
        }

        private string _amount;
        public string amount
        {
            get
            {
                return _amount.TrimIfNotNull();
            }
            set
            {
                _amount = value.TrimIfNotNull();
            }
        }
     
        public VirtualAccount virtual_account = new VirtualAccount();
    }

    public class VirtualAccount
    {
        private string _id;
        public string id
        {
            get
            {
                return _id.TrimIfNotNull();
            }
            set
            {
                _id = value.TrimIfNotNull();
            }
        }

        private string _virtual_account_number;
        public string virtual_account_number
        {
            get
            {
                return _virtual_account_number.TrimIfNotNull();
            }
            set
            {
                _virtual_account_number = value.TrimIfNotNull();
            }
        }

        private string _virtual_ifsc_number;
        public string virtual_ifsc_number
        {
            get
            {
                return _virtual_ifsc_number.TrimIfNotNull();
            }
            set
            {
                _virtual_ifsc_number = value.TrimIfNotNull();
            }
        }
    }
}