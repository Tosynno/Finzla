using Finzla.Application.Dtos;
using FluentValidation;

namespace Finzla.Application.Validators
{
    public sealed class IngestTransactionRequestValidator : AbstractValidator<IngestTransactionRequest>
    {
        private static readonly string[] AllowedTypes = ["Credit", "Debit"];

        public IngestTransactionRequestValidator()
        {
            RuleFor(x => x.TraceId)
                .NotEmpty().WithMessage("TraceId is required.")
                .MaximumLength(50).WithMessage("TraceId must not exceed 50 characters.");

            //When(x => x.IsOtherBank == true, () =>
            //{
            //    RuleFor(x => x.GLAccount)
            //        .NotEmpty().WithMessage("GLAccount is required for other bank transactions.")
            //        .MaximumLength(25).WithMessage("GLAccount must not exceed 25 characters.")
            //        .Matches(@"^\d{1,25}$").WithMessage("GLAccount must be numeric and up to 25 digits.");

            //    RuleFor(x => x.DebitAccount)
            //        .Empty().WithMessage("DebitAccount should not be provided when IsOtherBank is true.");
            //});

            When(x => x.IsOtherBank != true, () =>
            {
                RuleFor(x => x.DebitAccount)
                    .NotEmpty().WithMessage("DebitAccount is required for Finzla to Finzla account transactions.")
                    .MaximumLength(128).WithMessage("DebitAccount must not exceed 128 characters.");
            });

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