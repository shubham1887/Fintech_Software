using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class ReportRequest
    {
        private int _TakeCount;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }

        private string _Search;

        public string Search
        {
            get => _Search.TrimIfNotNull();
            set => _Search = value.TrimIfNotNull();
        }

        private string _SortColumnName;

        public string SortColumnName
        {
            get => _SortColumnName.TrimIfNotNull();
            set => _SortColumnName = value.TrimIfNotNull();
        }

        private string _SortDirection;

        public string SortDirection
        {
            get => _SortDirection.TrimIfNotNull();
            set => _SortDirection = value.TrimIfNotNull();
        }

        private bool _IsDMTReport;
        private bool _IsRechargeReport;
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _FromDate;

        public string FromDate
        {
            get => _FromDate.TrimIfNotNull();
            set => _FromDate = value.TrimIfNotNull();
        }

        private string _ToDate;

        public string ToDate
        {
            get => _ToDate.TrimIfNotNull();
            set => _ToDate = value.TrimIfNotNull();
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        public bool isLast10
        {
            get;
            set;
        }

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }


        private bool _IsPDFDownload;
        public bool IsPDFDownload
        {
            get => _IsPDFDownload;
            set => _IsPDFDownload = value;
        }

        private string _ExportType;

        public string ExportType
        {
            get => _ExportType.TrimIfNotNull();
            set => _ExportType = value.TrimIfNotNull();
        }

        public bool IsDMTReport
        {
            get => _IsDMTReport;
            set => _IsDMTReport = value;
        }

        public bool IsRechargeReport
        {
            get => _IsRechargeReport;
            set => _IsRechargeReport = value;
        }

        private string _Criteria;

        public string Criteria
        {
            get => _Criteria.TrimIfNotNull();
            set => _Criteria = value.TrimIfNotNull();
        }

        private string _CriteriaText;

        public string CriteriaText
        {
            get => _CriteriaText.TrimIfNotNull();
            set => _CriteriaText = value.TrimIfNotNull();
        }

        private string _CriteriaTextAmount;

        public string CriteriaTextAmount
        {
            get => _CriteriaTextAmount.TrimIfNotNull();
            set => _CriteriaTextAmount = value.TrimIfNotNull();
        }

        private decimal _MinAmount;

        public decimal MinAmount
        {
            get => _MinAmount;
            set => _MinAmount = value;
        }

        private decimal _MaxAmount;

        public decimal MaxAmount
        {
            get => _MaxAmount;
            set => _MaxAmount = value;
        }

        private bool _IsPayoutReport;

        public bool IsPayoutReport
        {
            get => _IsPayoutReport;
            set => _IsPayoutReport = value;
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNullWithDefault("All");
            set => _Status = value.TrimIfNotNullWithDefault("All");
        }

        private string _Complaint;

        public string Complaint
        {
            get => _Complaint.TrimIfNotNullWithDefault("All");
            set => _Complaint = value.TrimIfNotNullWithDefault("All");
        }

        private string _ReportBy;

        public string ReportBy
        {
            get => _ReportBy.TrimIfNotNullWithDefault("Inline");
            set => _ReportBy = value.TrimIfNotNullWithDefault("Inline");
        }

        internal string IsValidModalState()
        {
            var validator = new ReportRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success) return StatusType.Ok;

            return failures[0].ToString();
        }
    }

    internal class ReportRequestValidator : AbstractValidator<ReportRequest>
    {
        public ReportRequestValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty().WithMessage(TransactionMessage.InvalidDate);

            RuleFor(x => x.ToDate).NotEmpty().WithMessage(TransactionMessage.InvalidDate);

            //RuleFor(x => x.UserID).NotEmpty().WithMessage(TransactionMessage.InvalidUser);

            RuleFor(x => x.Status).NotEmpty().WithMessage(TransactionMessage.InvalidStatus);
        }
    }
}