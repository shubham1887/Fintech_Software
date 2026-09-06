using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PanMitra
{
    public class AuthRequest
    {
        private string _ApplicationMode;

        public string ApplicationMode
        {
            get => _ApplicationMode.TrimIfNotNull();
            set => _ApplicationMode = value.TrimIfNotNull();
        }

        private string _ApplicationType;

        public string ApplicationType
        {
            get => _ApplicationType.TrimIfNotNull();
            set => _ApplicationType = value.TrimIfNotNull();
        }

        private string _Category;

        public string Category
        {
            get => _Category.TrimIfNotNull();
            set => _Category = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Gender;

        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }

        private string _Mobile;

        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _Email;

        public string Email
        {
            get => _Email.TrimIfNotNull();
            set => _Email = value.TrimIfNotNull();
        }

        private string _Mode;

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }

        private string _Consent;

        public string Consent
        {
            get => _Consent.TrimIfNotNull();
            set => _Consent = value.TrimIfNotNull();
        }

        private string _Redirect_URL;

        public string Redirect_URL
        {
            get => _Redirect_URL.TrimIfNotNull();
            set => _Redirect_URL = value.TrimIfNotNull();
        }

        private string _Order_ID;

        public string Order_ID
        {
            get => _Order_ID.TrimIfNotNull();
            set => _Order_ID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new AuthRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? "OK" : failures[0].ToString();
        }
    }

    internal class AuthRequestValidator : AbstractValidator<AuthRequest>
    {
        public AuthRequestValidator()
        {


        }
    }
}

