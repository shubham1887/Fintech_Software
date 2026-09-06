using Project.Processing;

namespace Project.Response.RichTextMedia
{
    public class TextMediaResponse
    {
        private string _Content;

        public string Content
        {
            get => _Content.TrimIfNotNull();
            set => _Content = value.TrimIfNotNull();
        }
        

        private string _SessionCode;

        public string SessionCode
        {
            get => _SessionCode.TrimIfNotNull();
            set => _SessionCode = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.ToTitleCase();
            set => _Address = value.ToTitleCase();
        }
        private string _AliasName;

        public string AliasName
        {
            get => _AliasName.ToTitleCase();
            set => _AliasName = value.ToTitleCase();
        }
        
        private string _PersonName;
        public string PersonName
        {
            get => _PersonName.ToTitleCase();
            set => _PersonName = value.ToTitleCase();
        }
        private string _Logo;
        public string Logo
        {
            get => _Logo.TrimIfNotNull();
            set => _Logo = value.TrimIfNotNull();
        }
        private string _YesBankLogo;
        public string YesBankLogo
        {
            get => _YesBankLogo.TrimIfNotNull();
            set => _YesBankLogo = value.TrimIfNotNull();
        }
        
    }
}