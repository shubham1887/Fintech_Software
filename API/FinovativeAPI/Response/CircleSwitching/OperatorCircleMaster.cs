namespace Project.Response.CircleSwitching
{
    public class OperatorCircleMaster
    {
        public int CircleID { get; set; }

        public string CircleName { get; set; }

        public bool IsActive { get; set; }

        private string _Opcode;

        public string Opcode
        {
            get => _Opcode;
            set => _Opcode = value;
        }

        public int ProviderOperatorCircleMasterID { get; set; }
    }
}