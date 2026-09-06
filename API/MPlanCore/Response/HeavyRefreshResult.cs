using MplanCore.Controller;

namespace MplanCore.Response
{
    public class HeavyRefreshResult
    {
        private string _tel;

        public string tel
        {
            get => _tel.TrimIfNotNull();
            set => _tel = value.TrimIfNotNull();
        }

        private string _opName;

        public string opName
        {
            get => _opName.TrimIfNotNull();
            set => _opName = value.TrimIfNotNull();
        }

        private string _desc;

        public string desc
        {
            get => _desc.TrimIfNotNull();
            set => _desc = value.TrimIfNotNull();
        }

        private string _customerName;

        public string customerName
        {
            get => _customerName.TrimIfNotNull();
            set => _customerName = value.TrimIfNotNull();
        }
    }
}