using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.Onboarding
{
    public class OnboardingWrapper
    {
        public List<Project.Response.Onboarding.OnboardingViewModel> list = new List<Project.Response.Onboarding.OnboardingViewModel>();

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }
        private string _ReportPath;

        public string ReportPath
        {
            get => _ReportPath.TrimIfNotNull();
            set => _ReportPath = value.TrimIfNotNull();
        }
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }
        private bool _IsDownload;
        public bool IsDownload
        {
            get => _IsDownload;
            set => _IsDownload = value;
        }
    }
}
