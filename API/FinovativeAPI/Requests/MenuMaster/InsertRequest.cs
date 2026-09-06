using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.MenuMaster
{
    public class InsertRequest
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Url;

        public string Url
        {
            get => _Url.TrimIfNotNull();
            set => _Url = value.TrimIfNotNull();
        }

        private string _Icon;

        public string Icon
        {
            get => _Icon.TrimIfNotNull();
            set => _Icon = value.TrimIfNotNull();
        }

        private int _ParentMenuID;

        public int ParentMenuID
        {
            get => _ParentMenuID;
            set => _ParentMenuID = value;
        }

        private bool _IsAdmin;

        public bool IsAdmin
        {
            get => _IsAdmin;
            set => _IsAdmin = value;
        }

        private bool _IsSuperDistributer;

        public bool IsSuperDistributer
        {
            get => _IsSuperDistributer;
            set => _IsSuperDistributer = value;
        }

        private bool _IsDistributer;

        public bool IsDistributer
        {
            get => _IsDistributer;
            set => _IsDistributer = value;
        }

        private bool _IsRetailer;

        public bool IsRetailer
        {
            get => _IsRetailer;
            set => _IsRetailer = value;
        }

        private bool _IsAPIUser;

        public bool IsAPIUser
        {
            get => _IsAPIUser;
            set => _IsAPIUser = value;
        }

        private bool _IsSupport;

        public bool IsSupport
        {
            get => _IsSupport;
            set => _IsSupport = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private int _MenuSequence;

        public int MenuSequence
        {
            get => _MenuSequence;
            set => _MenuSequence = value;
        }

        private int _Level;
        public int Level { get => _Level; set => _Level = value; }

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
            RuleFor(x => x.Name)
              .Must(new Validation().IsValidString)
              .WithMessage(MenuMessages.InvalidName);

            RuleFor(x => x.Url)
             .Must(new Validation().IsValidString)
             .WithMessage(MenuMessages.InvalidURL);

            RuleFor(x => x.Description)
             .Must(new Validation().IsValidString)
             .WithMessage(MenuMessages.InvalidDescription);
        }
    }
}