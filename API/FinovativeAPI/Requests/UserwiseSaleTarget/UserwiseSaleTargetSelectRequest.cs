using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.Requests.UserwiseSaleTarget
{
    public class UserwiseSaleTargetSelectRequest
    {
        private int _Month;

        public int Month
        {
            get => _Month;
            set => _Month = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UserwiseSaleTargetSelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            if (success == false)
            {
                return failures[0].ToString();
            }

            DateTime current = DateTime.Now;
            int cMonth = current.Month;

            if (Month > cMonth)
            {
                return "Sales should be past(old) month(s) only";
            }

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UserwiseSaleTargetSelectRequestValidator : AbstractValidator<UserwiseSaleTargetSelectRequest>
    {
        public UserwiseSaleTargetSelectRequestValidator()
        {
            RuleFor(invoice => invoice.Month)
                      .Must(new Validation().IsValidInt)
                      .WithMessage(InvoiceMessages.InvalidMonth);
        }
    }
}