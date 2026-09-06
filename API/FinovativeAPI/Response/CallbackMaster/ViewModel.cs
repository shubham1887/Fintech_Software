using Project.Processing;
using System;

namespace Project.Response.CallbackMaster
{
    public class ViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _CallbackMasterID;

        public int CallbackMasterID
        {
            get => _CallbackMasterID;
            set => _CallbackMasterID = value;
        }

        private string _CallbackURL;

        public string CallbackURL
        {
            get => _CallbackURL.TrimIfNotNull();
            set => _CallbackURL = value.TrimIfNotNull();
        }

        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull();
            set => _Date = value.TrimIfNotNull();
        }

        private DateTime _UpdatedDate;

        public DateTime UpdatedDate
        {
            get => _UpdatedDate;
            set => _UpdatedDate = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }
    }
}