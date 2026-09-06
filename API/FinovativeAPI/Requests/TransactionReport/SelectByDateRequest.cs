using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.TransactionReport
{
    public class SelectByDateRequest
    {
        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull(); set => _Date = value.TrimIfNotNull();
        }

        private DateTime _DateModified;

        internal DateTime DateModified
        {
            get => _DateModified;
            set => _DateModified = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByDateRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }

            try
            {
                DateModified = new Util().ParseRequestDate(Date);
            }
            catch (Exception)
            {
                return Messages.InvalidDate;
            }

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByDateRequestValidator : AbstractValidator<SelectByDateRequest>
    {
        public SelectByDateRequestValidator()
        {
            RuleFor(x => x.Date)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.DaybookReport.InvalidtoDate);
        }
    }
}