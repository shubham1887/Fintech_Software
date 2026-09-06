using FluentValidation;
using FluentValidation.Results;
using MplanCore.Controller;
using Project.Message;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class SelectInActiveUsersRequest
    {
        private DateTime _FromDate;

        public DateTime FromDate { get => _FromDate; set => _FromDate = value; }
        public int UserTypeID { get => _UserTypeID; set => _UserTypeID = value; }

        private int _UserTypeID;

        public int UserID { get => _UserID; set => _UserID = value; }

        private int _UserID;

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

        internal string IsValidModalState()
        {
            var validator = new ReportRequestValidator();
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

    internal class ReportRequestValidator : AbstractValidator<SelectInActiveUsersRequest>
    {
        public ReportRequestValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty().WithMessage(LogMessage.InvalidDate);
        }
    }
}