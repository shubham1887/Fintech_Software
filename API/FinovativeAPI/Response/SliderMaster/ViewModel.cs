using Project.Processing;

namespace Project.Response.SliderMaster
{
    public class ViewModel
    {
        private string _ImagePath;

        public string ImagePath
        {
            get => _ImagePath.TrimIfNotNull();
            set => _ImagePath = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private int _SliderMasterID;

        public int SliderMasterID
        {
            get => _SliderMasterID;
            set => _SliderMasterID = value;
        }
    }
}