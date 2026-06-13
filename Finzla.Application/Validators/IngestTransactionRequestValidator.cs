using Finzla.Application.Dtos;
using FluentValidation;

namespace Finzla.Application.Validators
{
    public sealed class IngestTransactionRequestValidator : AbstractValidator<IngestTransactionRequest>
    {
        private static readonly string[] AllowedTypes = ["Credit", "Debit"];

        public IngestTransactionRequestValidator()
        {
            RuleFor(x => x.ExternalId)
                .NotEmpty().WithMessage("ExternalId is required.")
                .MaximumLength(256).WithMessage("ExternalId must not exceed 256 characters.");

            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("AccountId is required.")
                .MaximumLength(128).WithMessage("AccountId must not exceed 128 characters.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3, 8).WithMessage("Currency must be between 3 and 8 characters (e.g. NGN, USD).")
                .Matches("^[A-Za-z]+$").WithMessage("Currency must contain only letters.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type is required.")
                .Must(t => AllowedTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Type must be 'Credit' or 'Debit'.");

            RuleFor(x => x.OccurredAt)
                .NotEmpty().WithMessage("OccurredAt is required.")
                .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
                .WithMessage("OccurredAt cannot be in the future.");
        }
    }
}
