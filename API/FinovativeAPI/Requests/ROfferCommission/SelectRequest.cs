namespace Project.Requests.ROfferCommission
{
    public class SelectRequest
    {
        private int _SlabMasterID;

        public int SlabMasterID
        {
            get => _SlabMasterID;
            set => _SlabMasterID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
    }
}