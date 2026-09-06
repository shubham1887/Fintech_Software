using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Fund
{
    public class TransferReportRequest
    {
        private string _Type;

        private string _FromDate;

        public string FromDate
        {
            get => _FromDate.TrimIfNotNull();
            set => _FromDate = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _ToDate;

        public string ToDate
        {
            get => _ToDate.TrimIfNotNull();
            set => _ToDate = value.TrimIfNotNull();
        }

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private bool _IsExcelDownload;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }

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
        private int _TakeCount;
        private int _SkipCount;
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

        private string _ReportFor;

        public string ReportFor
        {
            get => _ReportFor.TrimIfNotNull();
            set => _ReportFor = value.TrimIfNotNull();
        }

        private string _PayMode;

        public string PayMode
        {
            get => _PayMode.TrimIfNotNullWithDefault("ALL");
            set => _PayMode = value.TrimIfNotNullWithDefault("ALL");
        }


        private string _CriteriaText;

        public string CriteriaText
        {
            get => _CriteriaText.TrimIfNotNull();
            set => _CriteriaText = value.TrimIfNotNull();
        }


        private string _Search;

        public string Search
        {
            get => _Search.TrimIfNotNull();
            set => _Search = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new TransferReportRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success)
                return StatusType.Ok;

            return failures[0].ToString();
        }
    }

    internal class TransferReportRequestValidator : AbstractValidator<TransferReportRequest>
    {
        public TransferReportRequestValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty().WithMessage(FundMessage.Invalidfrom);

            RuleFor(x => x.ToDate).NotEmpty().WithMessage(FundMessage.InvalidDate);

            RuleFor(x => x.Status).NotEmpty().WithMessage(FundMessage.InvalidStatus);
        }
    }
}