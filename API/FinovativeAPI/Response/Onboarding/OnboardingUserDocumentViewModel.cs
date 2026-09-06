using Project.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Project.Processing;

namespace Project.Response.Onboarding
{
    public class OnboardingUserDocumentViewModel
    {
        private int _OnboardingUserDocumentID;
        public int OnboardingUserDocumentID
        {
            get => _OnboardingUserDocumentID;
            set => _OnboardingUserDocumentID = value;
        }

        private int _UserID;
        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }


        private string _Ref_ID;
        public string Ref_ID
        {
            get => _Ref_ID.TrimIfNotNull();
            set => _Ref_ID = value.TrimIfNotNull();
        }

        private int _Type;
        public int Type
        {
            get => _Type;
            set => _Type = value;
        }

        private string _OldImage;

        public string OldImage
        {
            get => _OldImage.TrimIfNotNull();
            set => _OldImage = value.TrimIfNotNull();
        }

        private string _NewImage;
   
        public string NewImage
        {
            get => _NewImage.TrimIfNotNull();
            set => _NewImage = value.TrimIfNotNull();
        }

        private int _OnboardingUserID;
        public int OnboardingUserID
        {
            get => _OnboardingUserID;
            set => _OnboardingUserID = value;
        }


    }
}
