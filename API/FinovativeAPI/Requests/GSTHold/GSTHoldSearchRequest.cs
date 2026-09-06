using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.Requests.GSTHold
{
    public class GSTHoldSearchRequest
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

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        internal string IsValidModalState()
        {
            var validator = new GSTHoldSearchRequestValidator();
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
                return "Invoice should be past(old) month(s) only";
            }

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class GSTHoldSearchRequestValidator : AbstractValidator<GSTHoldSearchRequest>
    {
        public GSTHoldSearchRequestValidator()
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