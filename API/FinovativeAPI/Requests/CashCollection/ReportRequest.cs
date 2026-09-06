using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CashCollection
{
    public class ReportRequest
    {
        private int _UserID;
        private int _TakeCount;
        private int _SkipCount;
        private string _FromDate;
        private string _ToDate;

        public string FromDate
        {
            get => _FromDate.TrimIfNotNull();
            set => _FromDate = value.TrimIfNotNull();
        }

        public string ToDate
        {
            get => _ToDate.TrimIfNotNull();
            set => _ToDate = value.TrimIfNotNull();
        }

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

        public int UserID { get => _UserID; set => _UserID = value; }

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
        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNullWithDefault("All");
            set => _Type = value.TrimIfNotNullWithDefault("All");
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

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ReportRequestValidator : AbstractValidator<ReportRequest>
    {
        public ReportRequestValidator()
        {
            RuleFor(x => x.FromDate)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.Report.InvalidFromDate);

            RuleFor(x => x.ToDate)
               .Must(new Validation().IsValidString)
               .WithMessage(Message.Report.InvalidToDate);
        }
    }
}