using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.Tax
{
    public class SelectByCategoryRequest
    {
        private string _ServiceCategory;

        public string ServiceCategory
        {
            get => _ServiceCategory.TrimIfNotNull();
            set => _ServiceCategory = value.TrimIfNotNull();
        }

        private string _InvoiceNo;

        public string InvoiceNo
        {
            get => _InvoiceNo.TrimIfNotNull();
            set => _InvoiceNo = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
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

        private bool _IsDownload;

        public bool IsDownload
        {
            get => _IsDownload;
            set => _IsDownload = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByCategoryRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success == false)
                return failures[0].ToString();

            DateTime current = DateTime.Now;
            int cMonth = current.Month;
            int cYear = current.Year;
            if (Year > cYear)
                return $"Year should less than or equal to {cYear}";

            if (Year == cYear && Month >= cMonth)
                return "Invoice should be past(old) month(s) only";

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByCategoryRequestValidator : AbstractValidator<SelectByCategoryRequest>
    {
        public SelectByCategoryRequestValidator()
        {
            RuleFor(x => x.Month)
                     .Must(new Validation().IsValidInt)
                     .WithMessage(InvoiceMessages.InvalidMonth);

            RuleFor(x => x.Year)
                    .Must(new Validation().IsValidInt)
                    .WithMessage(InvoiceMessages.InvalidYear);

            RuleFor(x => x.ServiceCategory)
                   .Must(new Validation().IsValidString)
                   .WithMessage(InvoiceMessages.InvalidType);
        }
    }
}