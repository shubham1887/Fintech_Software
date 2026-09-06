using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.Requests.Invoice
{
    public class SelectRequest
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

        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
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

    internal class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
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