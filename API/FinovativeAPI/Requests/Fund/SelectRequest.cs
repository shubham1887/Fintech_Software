using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Fund
{
    public class SelectRequest
    {
        private int _RequestTo;
        private string _Type;//OWN or DOWNLINE

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

        private int _TakeCount;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        private string _Search;

        public string Search
        {
            get => _Search;
            set => _Search = value;
        }

        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }

        private string _DOCType;

        public string DOCType
        {
            get => _DOCType.TrimIfNotNull();
            set => _DOCType = value.TrimIfNotNull();
        }

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        public int RequestTo
        {
            get => _RequestTo;
            set => _RequestTo = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        public bool IsExcelDownload { get => isExcelDownload; set => isExcelDownload = value; }

        private bool isExcelDownload;

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
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
            get => _PayMode.TrimIfNotNullWithDefault("All");
            set => _PayMode = value.TrimIfNotNullWithDefault("All");
        }

        internal string IsValidModalState()
        {
            var validator = new ReportRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success)
            {
                return StatusType.Ok;
            }

            return failures[0].ToString();
        }
    }

    internal class ReportRequestValidator : AbstractValidator<SelectRequest>
    {
        public ReportRequestValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty().WithMessage(FundMessage.Invalidfrom);

            RuleFor(x => x.ToDate).NotEmpty().WithMessage(FundMessage.InvalidDate);

            RuleFor(x => x.Status).NotEmpty().WithMessage(FundMessage.InvalidStatus);

            RuleFor(x => x.Type).NotEmpty().WithMessage(FundMessage.InvalidType);
        }
    }
}