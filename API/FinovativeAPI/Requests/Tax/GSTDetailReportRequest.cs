using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Tax
{
    public class GSTDetailReportRequest
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

        internal string IsValidModalState()
        {
            var validator = new GSTDetailReportRequestValidator();
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

    internal class GSTDetailReportRequestValidator : AbstractValidator<GSTDetailReportRequest>
    {
        public GSTDetailReportRequestValidator()
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