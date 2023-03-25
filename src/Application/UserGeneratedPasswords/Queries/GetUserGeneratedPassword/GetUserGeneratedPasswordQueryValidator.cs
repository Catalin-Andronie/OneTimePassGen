using FluentValidation;

namespace OneTimePassGen.Application.UserGeneratedPasswords.Queries.GetUserGeneratedPassword;

internal sealed class GetUserGeneratedPasswordQueryValidator
    : AbstractValidator<GetUserGeneratedPasswordQuery>
{
    public GetUserGeneratedPasswordQueryValidator()
    {
        RuleFor(f => f.GeneratedPasswordId)
            .NotNull().WithMessage(f => $"Field '{nameof(f.GeneratedPasswordId)}' is required.")
            .NotEmpty().WithMessage(f => $"Field '{nameof(f.GeneratedPasswordId)}' is required.");
    }
}