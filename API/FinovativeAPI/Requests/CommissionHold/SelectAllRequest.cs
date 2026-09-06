using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CommissionHold
{
    public class SelectAllRequest
    {
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

        private string _SortColumnName;

        public string SortColumnName
        {
            get => _SortColumnName.TrimIfNotNull();
            set => _SortColumnName = value.TrimIfNotNull();
        }

        private string _SortDirection;

        public string SortDirection
        {
            get => _SortDirection.TrimIfNotNull();
            set => _SortDirection = value.TrimIfNotNull();
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

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }



        internal string IsValidModalState()
        {
            var validator = new SelectAllRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success) return StatusType.Ok;

            return failures[0].ToString();
        }
    }

    internal class SelectAllRequestValidator : AbstractValidator<SelectAllRequest>
    {
        public SelectAllRequestValidator()
        {
            RuleFor(x => x.Month)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.Report.InvalidMonth);

            RuleFor(x => x.Year)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.Report.InvalidYear);
        }
    }
}