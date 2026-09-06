using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.Requests.TransactionReport
{
    public class GSTSummaryRequest
    {
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

        private int _UserID;
        public int UserID { get => _UserID; set => _UserID = value; }
        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        internal string IsValidModalState()
        {
            var validator = new GSTSummaryRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            if (success == false)
            {
                return failures[0].ToString();
            }

            DateTime current = DateTime.Now;
            int cMonth = current.Month;
            int cYear = current.Year;
            if (Year > cYear)
            {
                return $"Year should less than or equal to {cYear}";
            }

            if (Year == cYear && Month >= cMonth)
            {
                return "GST should be past(old) month(s) only";
            }

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class GSTSummaryRequestValidator : AbstractValidator<GSTSummaryRequest>
    {
        public GSTSummaryRequestValidator()
        {
            RuleFor(invoice => invoice.Month)
                      .Must(new Validation().IsValidInt)
                      .WithMessage(InvoiceMessages.InvalidMonth);
            RuleFor(config => config.Year)
                    .Must(new Validation().IsValidInt)
                    .WithMessage(InvoiceMessages.InvalidYear);
        }
    }
}