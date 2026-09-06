using Project.Processing;

namespace Project.Response.User

{
    public class BarCodeViewModel
    {
        private string _BarCodeImageURL;

        public string BarCodeImageURL
        {
            get => _BarCodeImageURL.TrimIfNotNull();
            set => _BarCodeImageURL = value.TrimIfNotNull();
        }

        private string _SetUpCode;

        public string SetUpCode
        {
            get => _SetUpCode.TrimIfNotNull();
            set => _SetUpCode = value.TrimIfNotNull();
        }

        private string _UserUniqueKey;

        public string UserUniqueKey
        {
            get => _UserUniqueKey.TrimIfNotNull();
            set => _UserUniqueKey = value.TrimIfNotNull();
        }
    }
}