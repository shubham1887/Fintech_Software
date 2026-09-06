using Project.Processing;

namespace Project.Response

{
    public class InstantPay_CallbackReturnReponse
    {
        private string _ipay_id;

        public string ipay_id
        {
            get => _ipay_id.TrimIfNotNull();
            set => _ipay_id = value.TrimIfNotNull();
        }

        private bool _success;

        public bool success
        {
            get => _success;
            set => _success = value;
        }

        private string _description;

        public string description
        {
            get => _description.TrimIfNotNull();
            set => _description = value.TrimIfNotNull();
        }
    }
}