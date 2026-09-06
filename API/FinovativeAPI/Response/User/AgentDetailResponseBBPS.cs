using Project.Processing;
using Project.Requests.User;
using System.Collections.Generic;

namespace Project.Response.User
{
    public class AgentDetailResponseBBPS
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private string _AgentName;

        public string AgentName
        {
            get => _AgentName.TrimIfNotNull();
            set => _AgentName = value.TrimIfNotNull();
        }

        private string _AgentShopName;

        public string AgentShopName
        {
            get => _AgentShopName.TrimIfNotNull();
            set => _AgentShopName = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }


        private string _Address;

        public string Address
        {
            get => _Address.ToTitleCase();
            set => _Address = value.ToTitleCase();
        }

        public int StateID { get; internal set; }



        private string _EffectiveFormDate;

        public string EffectiveFormDate
        {
            get => _EffectiveFormDate;
            set => _EffectiveFormDate = value;
        }


        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        private string _AgentInstituteID;

        public string AgentInstituteID
        {
            get => _AgentInstituteID.TrimIfNotNull();
            set => _AgentInstituteID = value.TrimIfNotNull();
        }

        private string _PaymentMode;

        public string PaymentMode
        {
            get => _PaymentMode.TrimIfNotNull();
            set => _PaymentMode = value.TrimIfNotNull();
        }

        private string _PaymentChannel;

        public string PaymentChannel
        {
            get => _PaymentChannel.TrimIfNotNull();
            set => _PaymentChannel = value.TrimIfNotNull();
        }

        private string _AgentReferenceId;

        public string AgentReferenceId
        {
            get => _AgentReferenceId.TrimIfNotNull();
            set => _AgentReferenceId = value.TrimIfNotNull();
        }


    }
}