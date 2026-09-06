using Project.Processing;

namespace Project.Response.ECollection
{
    public class NotifyApiResponse
    {
        public Notify notify =new Notify();
        public class Notify
        {
            private string _customer_code;
            public string customer_code
            {
                get => _customer_code.TrimIfNotNull();
                set => _customer_code = value.TrimIfNotNull();
            }

            private string _bene_account_no;
            public string bene_account_no
            {
                get => _bene_account_no.TrimIfNotNull();
                set => _bene_account_no = value.TrimIfNotNull();
            }

            private string _bene_account_ifsc;
            public string bene_account_ifsc
            {
                get => _bene_account_ifsc.TrimIfNotNull();
                set => _bene_account_ifsc = value.TrimIfNotNull();
            }

            private string _bene_full_name;
            public string bene_full_name
            {
                get => _bene_full_name.TrimIfNotNull();
                set => _bene_full_name = value.TrimIfNotNull();
            }

            private string _transfer_type;
            public string transfer_type
            {
                get => _transfer_type.TrimIfNotNull();
                set => _transfer_type = value.TrimIfNotNull();
            }

            private string _transfer_unique_no;
            public string transfer_unique_no
            {
                get => _transfer_unique_no.TrimIfNotNull();
                set => _transfer_unique_no = value.TrimIfNotNull();
            }

            private string _transfer_timestamp;
            public string transfer_timestamp
            {
                get => _transfer_timestamp.TrimIfNotNull();
                set => _transfer_timestamp = value.TrimIfNotNull();
            }

            private string _transfer_ccy;
            public string transfer_ccy
            {
                get => _transfer_ccy.TrimIfNotNull();
                set => _transfer_ccy = value.TrimIfNotNull();
            }

            private double _transfer_amt;
            public double transfer_amt
            {
                get => _transfer_amt;
                set => _transfer_amt = value;
            }

            private string _rmtr_account_no;
            public string rmtr_account_no
            {
                get => _rmtr_account_no.TrimIfNotNull();
                set => _rmtr_account_no = value.TrimIfNotNull();
            }

            private string _rmtr_account_ifsc;
            public string rmtr_account_ifsc
            {
                get => _rmtr_account_ifsc.TrimIfNotNull();
                set => _rmtr_account_ifsc = value.TrimIfNotNull();
            }

            private string _rmtr_account_type;
            public string rmtr_account_type
            {
                get => _rmtr_account_type.TrimIfNotNull();
                set => _rmtr_account_type = value.TrimIfNotNull();
            }

            private string _rmtr_full_name;
            public string rmtr_full_name
            {
                get => _rmtr_full_name.TrimIfNotNull();
                set => _rmtr_full_name = value.TrimIfNotNull();
            }

            private string _rmtr_to_bene_note;
            public string rmtr_to_bene_note
            {
                get => _rmtr_to_bene_note.TrimIfNotNull();
                set => _rmtr_to_bene_note = value.TrimIfNotNull();
            }

            private int _attempt_no;
            public int attempt_no
            {
                get => _attempt_no;
                set => _attempt_no = value;
            }

            private string _status;
            public string status
            {
                get => _status.TrimIfNotNull();
                set => _status = value.TrimIfNotNull();
            }

            private string _credit_acct_no;
            public string credit_acct_no
            {
                get => _credit_acct_no.TrimIfNotNull();
                set => _credit_acct_no = value.TrimIfNotNull();
            }

            private string _credited_at;
            public string credited_at
            {
                get => _credited_at.TrimIfNotNull();
                set => _credited_at = value.TrimIfNotNull();
            }

        }
    }
}
