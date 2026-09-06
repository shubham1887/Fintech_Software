using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.Requests.UserwiseSaleTarget
{
    public class SaleTargetSelectRequest
    {
        private int _Month;

        public int Month
        {
            get => _Month;
            set => _Month = value;
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

        internal string IsValidModalState()
        {
            var validator = new SaleTargetSelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            if (success == false)
            {
                return failures[0].ToString();
            }
            if (Month <= 0)
            {
                Month = DateTime.Now.AddMonths(-1).Month;
            }

            DateTime current = DateTime.Now;
            int cMonth = current.Month;

            if (Month > cMonth && cMonth != 1)
            {
                return "Sales should be past(old) month(s) only";
            }

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SaleTargetSelectRequestValidator : AbstractValidator<SaleTargetSelectRequest>
    {
        public SaleTargetSelectRequestValidator()
        {
            RuleFor(a => a.UserTypeID)
                      .Must(new Validation().IsValidInt)
                      .WithMessage(UserwiseSaleTargetMessages.InvalidRole);
        }
    }
}