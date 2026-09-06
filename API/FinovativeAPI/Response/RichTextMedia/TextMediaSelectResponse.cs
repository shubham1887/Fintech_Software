using Project.Processing;

namespace Project.Response.RichTextMedia
{
    public class TextMediaSelectResponse
    {
        private int _ID;

        public int ID
        {
            get => _ID;
            set => _ID = value;
        }

        private string _Key;

        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }

        private string _Content;

        public string Content
        {
            get => _Content.TrimIfNotNull();
            set => _Content = value.TrimIfNotNull();
        }
    }
}