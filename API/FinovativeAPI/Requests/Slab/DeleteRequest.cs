using FluentValidation;
using FluentValidation.Results;
using Project.DAL;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Slab
{
    public class DeleteRequest
    {
        private int _SlabID;

        public int SlabID { get => _SlabID; set => _SlabID = value; }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            if (!success)
            {
                return failures[0].ToString();
            }
            success = IsValidForDelete();
            if (!success)
            {
                return Message.SlabMessage.UnabletoDelete;
            }
            return StatusType.Ok;
        }

        private bool IsValidForDelete()
        {
            bool success = new UserSlabMappingDal().IsValidForDelete(SlabID);
            return success;
        }

        public override string ToString()
        {
            return $"Deleted SlabID: {SlabID}";
        }
    }

    internal class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.SlabID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}