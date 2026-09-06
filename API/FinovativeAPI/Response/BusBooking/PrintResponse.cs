using Project.Processing;

namespace Project.Response.BusBooking

{
    public class PrintResponse
    {
        private string _BusType;

        public string BusType
        {
            get => _BusType.TrimIfNotNull();
            set => _BusType = value.TrimIfNotNull();
        }

        private string _destinationCity;

        public string destinationCity
        {
            get => _destinationCity.TrimIfNotNull();
            set => _destinationCity = value.TrimIfNotNull();
        }

        private string _dropLocation;

        public string dropLocation
        {
            get => _dropLocation.TrimIfNotNull();
            set => _dropLocation = value.TrimIfNotNull();
        }

        private string _pickupLocation;

        public string pickupLocation
        {
            get => _pickupLocation.TrimIfNotNull();
            set => _pickupLocation = value.TrimIfNotNull();
        }

        private string _address;

        public string address
        {
            get => _address.TrimIfNotNull();
            set => _address = value.TrimIfNotNull();
        }

        private string _age;

        public string age
        {
            get => _age.TrimIfNotNull();
            set => _age = value.TrimIfNotNull();
        }

        private string _email;

        public string email
        {
            get => _email.TrimIfNotNull();
            set => _email = value.TrimIfNotNull();
        }

        private string _gender;

        public string gender
        {
            get => _gender.TrimIfNotNull();
            set => _gender = value.TrimIfNotNull();
        }

        private string _mobile;

        public string mobile
        {
            get => _mobile.TrimIfNotNull();
            set => _mobile = value.TrimIfNotNull();
        }

        private string _name;

        public string name
        {
            get => _name.TrimIfNotNull();
            set => _name = value.TrimIfNotNull();
        }

        private string _amount;

        public string amount
        {
            get => _amount.TrimIfNotNull();
            set => _amount = value.TrimIfNotNull();
        }

        private string _base_fare;

        public string base_fare
        {
            get => _base_fare.TrimIfNotNull();
            set => _base_fare = value.TrimIfNotNull();
        }

        private string _comm;

        public string comm
        {
            get => _comm.TrimIfNotNull();
            set => _comm = value.TrimIfNotNull();
        }

        private string _tds;

        public string tds
        {
            get => _tds.TrimIfNotNull();
            set => _tds = value.TrimIfNotNull();
        }

        private string _total_deduction;

        public string total_deduction
        {
            get => _total_deduction.TrimIfNotNull();
            set => _total_deduction = value.TrimIfNotNull();
        }

        private string _AEPSResponseJSON;

        public string AEPSResponseJSON
        {
            get => _AEPSResponseJSON.TrimIfNotNull();
            set => _AEPSResponseJSON = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _CompanyName;

        public string CompanyName
        {
            get => _CompanyName.TrimIfNotNull();
            set => _CompanyName = value.TrimIfNotNull();
        }

        private string _refid;

        public string refid
        {
            get => _refid.TrimIfNotNull();
            set => _refid = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _customer_mobile;

        public string customer_mobile
        {
            get => _customer_mobile.TrimIfNotNull();
            set => _customer_mobile = value.TrimIfNotNull();
        }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        public int OperatorID { get; set; }

        private string _seatName;

        public string seatName
        {
            get => _seatName.TrimIfNotNull();
            set => _seatName = value.TrimIfNotNull();
        }

        private string _eventName;

        public string eventName
        {
            get => _eventName.TrimIfNotNull();
            set => _eventName = value.TrimIfNotNull();
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _dropTime;

        public string dropTime
        {
            get => _dropTime.TrimIfNotNull();
            set => _dropTime = value.TrimIfNotNull();
        }

        private string _pickupTime;

        public string pickupTime
        {
            get => _pickupTime.TrimIfNotNull();
            set => _pickupTime = value.TrimIfNotNull();
        }

        internal int DistributorID;

        internal int SuperDistributorID;
    }
}