using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Tax
{
    public class TDSReportRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
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
        private int _Month;

        public int Month
        {
            get => _Month;
            set => _Month = value;
        }

        private int _Year;

        public int Year
        {
            get => _Year;
            set => _Year = value;
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private string _ReportBy;

        public string ReportBy
        {
            get => _ReportBy.TrimIfNotNullWithDefault("Inline");
            set => _ReportBy = value.TrimIfNotNullWithDefault("Inline");
        }

        internal string IsValidModalState()
        {
            var validator = new TDSReportRequestValidator();
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

    internal class TDSReportRequestValidator : AbstractValidator<TDSReportRequest>
    {
        public TDSReportRequestValidator()
        {
            RuleFor(x => x.Month)
                   .Must(new Validation().IsValidInt)
                   .WithMessage(InvoiceMessages.InvalidMonth);

            RuleFor(x => x.Year)
                    .Must(new Validation().IsValidInt)
                    .WithMessage(InvoiceMessages.InvalidYear);
        }
    }
}