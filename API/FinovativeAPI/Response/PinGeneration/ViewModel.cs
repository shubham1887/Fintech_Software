using Project.Processing;
using System;

namespace Project.Response.PinGeneration
{
    public class ViewModel
    {

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        
        private string _DeviceID;

        public string DeviceID
        {
            get => _DeviceID.TrimIfNotNull();
            set => _DeviceID = value.TrimIfNotNull();
        }
        
        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }
        
        private int _Mpin;

        public int Mpin
        {
            get => _Mpin;
            set => _Mpin = value;
        }

        private string _DOC;
        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private int _DOCInt;
        public int DOCInt
        {
            get => _DOCInt;
            set => _DOCInt = value;
        }

        public DateTime DOCDateTime
        {
            get
            {
                DateTime.TryParse(DOC, out DateTime dateTime);
                return dateTime;
            }
        }

    }
}
