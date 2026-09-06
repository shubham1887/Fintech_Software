using Project.Processing;

namespace Project.Response.CallbackMaster
{
    public class EasebuzzCallBack
    {
        private string _txnid;

        public string txnid
        {
            get => _txnid.TrimIfNotNull();
            set => _txnid = value.TrimIfNotNull();
        }

        private string _firstname;

        public string firstname
        {
            get => _firstname.TrimIfNotNull();
            set => _firstname = value.TrimIfNotNull();
        }

        private string _email;

        public string email
        {
            get => _email.TrimIfNotNull();
            set => _email = value.TrimIfNotNull();
        }

        private string _phone;

        public string phone
        {
            get => _phone.TrimIfNotNull();
            set => _phone = value.TrimIfNotNull();
        }

        private string _key;

        public string key
        {
            get => _key.TrimIfNotNull();
            set => _key = value.TrimIfNotNull();
        }

        private string _mode;

        public string mode
        {
            get => _mode.TrimIfNotNull();
            set => _mode = value.TrimIfNotNull();
        }

        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _payment_source;

        public string payment_source
        {
            get => _payment_source.TrimIfNotNull();
            set => _payment_source = value.TrimIfNotNull();
        }

        private string _error;

        public string error
        {
            get => _error.TrimIfNotNull();
            set => _error = value.TrimIfNotNull();
        }

        private string _name_on_card;

        public string name_on_card
        {
            get => _name_on_card.TrimIfNotNull();
            set => _name_on_card = value.TrimIfNotNull();
        }

        private string _cardnum;

        public string cardnum
        {
            get => _cardnum.TrimIfNotNull();
            set => _cardnum = value.TrimIfNotNull();
        }

        private string _card_type;

        public string card_type
        {
            get => _card_type.TrimIfNotNull();
            set => _card_type = value.TrimIfNotNull();
        }

        private string _easepayid;

        public string easepayid
        {
            get => _easepayid.TrimIfNotNull();
            set => _easepayid = value.TrimIfNotNull();
        }

        private string _amount;

        public string amount
        {
            get => _amount.TrimIfNotNull();
            set => _amount = value.TrimIfNotNull();
        }

        private string _net_amount_debit;

        public string net_amount_debit
        {
            get => _net_amount_debit.TrimIfNotNull();
            set => _net_amount_debit = value.TrimIfNotNull();
        }

        private string _cash_back_percentage;

        public string cash_back_percentage
        {
            get => _cash_back_percentage.TrimIfNotNull();
            set => _cash_back_percentage = value.TrimIfNotNull();
        }

        private string _deduction_percentage;

        public string deduction_percentage
        {
            get => _deduction_percentage.TrimIfNotNull();
            set => _deduction_percentage = value.TrimIfNotNull();
        }

        private string _productinfo;

        public string productinfo
        {
            get => _productinfo.TrimIfNotNull();
            set => _productinfo = value.TrimIfNotNull();
        }

        private string _error_Message;

        public string error_Message
        {
            get => _error_Message.TrimIfNotNull();
            set => _error_Message = value.TrimIfNotNull();
        }

        private string _upi_va;

        public string upi_va
        {
            get => _upi_va.TrimIfNotNull();
            set => _upi_va = value.TrimIfNotNull();
        }

        private string _hash;

        public string hash
        {
            get => _hash.TrimIfNotNull();
            set => _hash = value.TrimIfNotNull();
        }

        private string _bank_ref_num;

        public string bank_ref_num
        {
            get => _bank_ref_num.TrimIfNotNull();
            set => _bank_ref_num = value.TrimIfNotNull();
        }
    }
}