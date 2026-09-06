using Project.Processing;

namespace Project.Response.Agent
{
    public class ViewModel
    {
        private string _AgentCode;

        public string AgentCode
        {
            get => _AgentCode.ToTitleCase();
            set => _AgentCode = value.ToTitleCase();
        }

        private string _Message;

        public string Message
        {
            get => _Message.ToTitleCase();
            set => _Message = value.ToTitleCase();
        }

        private bool _IsRegister;

        public bool IsRegister
        {
            get => _IsRegister;
            set => _IsRegister = value;
        }
    }
}