using Project.Processing;
using System;

namespace Project.Response.IPMaster
{
    public class ViewModel
    {
        private int _UserID;

        private int _IPMasterID;
        private string _IPAddress;

        public DateTime Date
        {
            get;
            set;
        }

        public DateTime UpdatedDate
        {
            get;
            set;
        }

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        public int IPMasterID
        {
            get => _IPMasterID;
            set => _IPMasterID = value;
        }

        public string IPAddress
        {
            get => _IPAddress.TrimIfNotNull();
            set => _IPAddress = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _FirmName;
        public string FirmName { get => _FirmName; set => _FirmName = value; }
    }
}