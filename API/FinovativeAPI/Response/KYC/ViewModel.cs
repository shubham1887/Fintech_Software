using Project.Processing;
using System;

namespace Project.Response.KYC
{
    public class ViewModel
    {
        private string _RequestDate;

        public string RequestDate
        {
            get => _RequestDate.TrimIfNotNull();
            set => _RequestDate = value.TrimIfNotNull();
        }

        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.TrimIfNotNull();
            set => _PersonName = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Parent;

        public string Parent
        {
            get => _Parent.TrimIfNotNull();
            set => _Parent = value.TrimIfNotNull();
        }

        private string _ApproveDate;

        public string ApproveDate
        {
            get => _ApproveDate.TrimIfNotNull();
            set => _ApproveDate = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private bool _ISApproved;

        public bool ISApproved
        {
            get => _ISApproved;
            set => _ISApproved = value;
        }

        private string _ApprovedImageURL;

        public string ApprovedImageURL
        {
            get => _ApprovedImageURL.TrimIfNotNull();
            set => _ApprovedImageURL = value.TrimIfNotNull();
        }

        private string _SampleImageURL;

        public string SampleImageURL
        {
            get => _SampleImageURL.TrimIfNotNull();
            set => _SampleImageURL = value.TrimIfNotNull();
        }

        private string _DocumentType;

        public string DocumentType
        {
            get => _DocumentType.TrimIfNotNull();
            set => _DocumentType = value.TrimIfNotNull();
        }

        private string _DocumentNo;

        public string DocumentNo
        {
            get => _DocumentNo.TrimIfNotNull();
            set => _DocumentNo = value.TrimIfNotNull();
        }

        private string _Document;

        public string Document
        {
            get => _Document.TrimIfNotNull();
            set => _Document = value.TrimIfNotNull();
        }

        private int _UserDocumentID;

        public int UserDocumentID
        {
            get => _UserDocumentID;
            set => _UserDocumentID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _WhatsAppNumber;

        public string WhatsAppNumber
        {
            get => _WhatsAppNumber.TrimIfNotNull();
            set => _WhatsAppNumber = value.TrimIfNotNull();
        }      

        public string CompactResponse
        {
            get => $"Document No : " + DocumentNo + Environment.NewLine + "DocumentType : " + DocumentType + Environment.NewLine;
        }

        private string _Label;

        public string Label
        {
            get => _Label.TrimIfNotNull();
            set => _Label = value.TrimIfNotNull();
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _KYCApproveName;

        public string KYCApproveName
        {
            get => _KYCApproveName.TrimIfNotNull();
            set => _KYCApproveName = value.TrimIfNotNull();
        }


    }
}