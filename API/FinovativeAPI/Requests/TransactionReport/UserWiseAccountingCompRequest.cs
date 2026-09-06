using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.TransactionReport
{
    public class UserWiseAccountingCompRequest
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

        
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull();
            set => _Date = value.TrimIfNotNull();
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
            var validator = new UserWiseAccountingCompRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success) return StatusType.Ok;

            return failures[0].ToString();
        }
    }
    internal class UserWiseAccountingCompRequestValidator : AbstractValidator<UserWiseAccountingCompRequest>
    {
        public UserWiseAccountingCompRequestValidator()
        {
            RuleFor(x => x.Date).NotEmpty().WithMessage(TransactionMessage.InvalidDate);
            RuleFor(x => x.Status).NotEmpty().WithMessage(TransactionMessage.InvalidStatus);
        }
    }
}
