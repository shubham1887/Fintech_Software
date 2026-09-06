namespace Project.Response.eKYC
{
    public class Quick_eKYCInitiateResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message;
            set => _Message = value;
        }

        private int _eKYCMasterID;

        public int eKYCMasterID
        {
            get => _eKYCMasterID;
            set => _eKYCMasterID = value;
        }
    }
}