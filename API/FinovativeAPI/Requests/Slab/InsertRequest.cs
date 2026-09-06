using FluentValidation;
using FluentValidation.Results;
using Project.DAL;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Slab
{
    public class InsertRequest
    {
        private string _SlabName;

        public string SlabName
        {
            get => _SlabName.TrimIfNotNull();
            set => _SlabName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            if (!success)
            {
                return failures[0].ToString();
            }
            success = IsDuplicateName();
            if (!success)

                return SlabMessage.DuplicateName;

            return StatusType.Ok;
        }

        private bool IsDuplicateName()
        {
            bool success = new SlabDAL().IsDuplicateName(SlabName);
            return success;
        }

        public override string ToString()
        {
            return $"Created new slab: {SlabName}";
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.SlabName)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}