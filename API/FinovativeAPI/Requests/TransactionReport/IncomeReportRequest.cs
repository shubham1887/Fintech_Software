using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.TransactionReport
{
    public class IncomeReportRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
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
        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
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

        internal string IsValidModalState()
        {
            var validator = new IncomeReportRequestValidator();
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

    internal class IncomeReportRequestValidator : AbstractValidator<IncomeReportRequest>
    {
        public IncomeReportRequestValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty().WithMessage(TransactionDetailMessage.Invalidfrom);

            RuleFor(x => x.ToDate).NotEmpty().WithMessage(TransactionDetailMessage.InvalidDate);

            RuleFor(x => x.UserID).NotEmpty().WithMessage(TransactionDetailMessage.InvalidUser);
        }
    }
}