using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.Tax
{
    public class GSTDetailRequest
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

        private int _TakeCount;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }

        private string _Search;

        public string Search
        {
            get => _Search.TrimIfNotNull();
            set => _Search = value.TrimIfNotNull();
        }

        private string _ServiceCategory;

        public string ServiceCategory
        {
            get => _ServiceCategory.TrimIfNotNull();
            set => _ServiceCategory = value.TrimIfNotNull();
        }

        private string _InvoiceType;

        public string InvoiceType
        {
            get => _InvoiceType.TrimIfNotNull();
            set => _InvoiceType = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new GSTDetailRequestValidator();
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

    internal class GSTDetailRequestValidator : AbstractValidator<GSTDetailRequest>
    {
        public GSTDetailRequestValidator()
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