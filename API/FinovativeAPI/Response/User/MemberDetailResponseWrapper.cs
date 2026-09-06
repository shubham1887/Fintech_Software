using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.User
{
    public class MemberDetailResponseWrapper
    {
        public int Count { get; set; }

        public List<MemberDetailResponse> data = new List<MemberDetailResponse>();
        public List<AgentDetailResponse> dataRes = new List<AgentDetailResponse>();
        public List<AgentDetailResponseBBPS> dataResBBPS = new List<AgentDetailResponseBBPS>();
        public List<Headerlist> headerlist = new List<Headerlist>();

        private BalanceHeaderViewModel headerSummary;

        public BalanceHeaderViewModel HeaderSummary
        {
            get => headerSummary;
            set => headerSummary = value;
        }

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private string _ReportPath;

        public string ReportPath
        {
            get => _ReportPath.TrimIfNotNull();
            set => _ReportPath = value.TrimIfNotNull();
        }
    }
}