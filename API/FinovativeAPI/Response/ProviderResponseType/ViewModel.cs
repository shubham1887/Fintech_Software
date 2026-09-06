using Project.Processing;

namespace Project.Response.ProviderResponseType
{
    public class ViewModel
    {
        private int _ProviderResponseTypeMasterID;
        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        public int ID
        {
            get => _ProviderResponseTypeMasterID;
            set => _ProviderResponseTypeMasterID = value;
        }
    }
}