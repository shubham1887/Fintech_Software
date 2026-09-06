using FluentValidation;

namespace FintechApp.Requests.ProviderGateway
{
    // Dry-runs a sample provider reply through the configured response rules (no HTTP call).
    public class TestProviderResponseRequest
    {
        public int ProviderID { get; set; }

        // TRANSACTION / BALANCE / STATUS — which rule group to evaluate against.
        public string RuleKind { get; set; } = string.Empty;

        public string SampleText { get; set; } = string.Empty;
    }

    public class TestProviderResponseRequestValidator : AbstractValidator<TestProviderResponseRequest>
    {
        public TestProviderResponseRequestValidator()
        {
            RuleFor(x => x.ProviderID).GreaterThan(0);
            RuleFor(x => x.SampleText).NotEmpty();
        }
    }
}
