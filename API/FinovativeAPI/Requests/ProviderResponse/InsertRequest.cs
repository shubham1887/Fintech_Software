using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderResponse
{
    public class InsertRequest
    {
        private int _ServiceID;
        private int _ProviderID;

        private string _Type;

        public string TextMustExists
        {
            get => _TextMustExists.TrimIfNotNull();
            set => _TextMustExists = value.TrimIfNotNull();
        }

        public string Response { get => _Response.TrimIfNotNull(); set => _Response = value.TrimIfNotNull(); }
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

        public bool IsRefCode { get => _IsRefCode; set => _IsRefCode = value; }
        public int ServiceID { get => _ServiceID; set => _ServiceID = value; }
        public int ProviderID { get => _ProviderID; set => _ProviderID = value; }

        public string Type { get => _Type; set => _Type = value; }

        private string _TextMustExists;
        private string _Response;

        private bool _IsActive;

        private bool _IsRefCode;

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.ServiceID)
               .Must(new Validation().IsValidInt)
               .WithMessage(ServiceMessage.NotFound);

            RuleFor(x => x.Type)
                .Must(new Validation().IsValidString)
                .WithMessage(ProviderResponseMessage.InvalidTextMustExists);

            RuleFor(x => x.Response)
                .Must(new Validation().IsValidString)
                .WithMessage(ProviderResponseMessage.InvalidResponse);
        }
    }
}