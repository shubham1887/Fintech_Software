using Project.Processing;

namespace Project.Response.PaymentModes
{
    public class ViewModel
    {
        private int _PaymentModeMasterID;
        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        public int PaymentModeMasterID
        {
            get => _PaymentModeMasterID;
            set => _PaymentModeMasterID = value;
        }
    }
}