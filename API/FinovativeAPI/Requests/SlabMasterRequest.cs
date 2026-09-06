using FluentValidation;
using Project.Processing;

namespace Project.Requests
{
    public class SlabMasterRequest
    {
        private string _SlabName;
        private int _SlabID;

        public string SlabName
        {
            get => _SlabName.TrimIfNotNull();
            set => _SlabName = value.TrimIfNotNull();
        }

        public int SlabID
        {
            get => _SlabID;
            set => _SlabID = value;
        }

        public bool IsDelete { get; set; }
        public bool IsActive { get; set; }
    }

    public class SlabMasterRequestValidator : AbstractValidator<SlabMasterRequest>
    {
        public SlabMasterRequestValidator()
        {
            RuleFor(x => x.SlabName).NotEmpty().WithMessage("Please enter slab name");
        }
    }
}