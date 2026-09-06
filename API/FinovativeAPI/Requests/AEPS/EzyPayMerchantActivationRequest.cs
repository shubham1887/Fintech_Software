using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.AEPS
{
    public class EzyPayMerchantActivationRequest
    {
        private string _RetailerID;

        public string RetailerID
        {
            get => _RetailerID.TrimIfNotNull();
            set => _RetailerID = value.TrimIfNotNull();
        }

        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _ShopName;

        public string ShopName
        {
            get => _ShopName.TrimIfNotNull();
            set => _ShopName = value.TrimIfNotNull();
        }

        private string _Pan;

        public string Pan
        {
            get => _Pan.TrimIfNotNull();
            set => _Pan = value.TrimIfNotNull();
        }

        private string _AdharNo;

        public string AdharNo
        {
            get => _AdharNo.TrimIfNotNull();
            set => _AdharNo = value.TrimIfNotNull();
        }

        private string _pincode;

        public string pincode
        {
            get => _pincode.TrimIfNotNull();
            set => _pincode = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        public CustomFile PANImage = new CustomFile();
        public CustomFile AadharFrontImage = new CustomFile();
        public CustomFile AadharBackImage = new CustomFile();

        private string _PanImage;

        public string PanImage
        {
            get => _PanImage.TrimIfNotNull();
            set => _PanImage = value.TrimIfNotNull();
        }

        private string _AdharImageFront;

        public string AdharImageFront
        {
            get => _AdharImageFront.TrimIfNotNull();
            set => _AdharImageFront = value.TrimIfNotNull();
        }

        private string _AdharImageBack;

        public string AdharImageBack
        {
            get => _AdharImageBack.TrimIfNotNull();
            set => _AdharImageBack = value.TrimIfNotNull();
        }
    }
}