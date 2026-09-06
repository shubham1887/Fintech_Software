using Project.Processing;

namespace Project.Response.DMTSettings
{
    public class TransferModeModel
    {
        private string _TransferMode;

        public string TransferMode
        {
            get => _TransferMode.TrimIfNotNull().ToUpperCase();
            set => _TransferMode = value.TrimIfNotNull().ToUpperCase();
        }
    }
}