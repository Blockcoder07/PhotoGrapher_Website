using FluentValidation;
using PhotographerApp.Core.DTOs;

namespace PhotographerApp.Core.Validators;

public class SubmitContactMessageRequestValidator : AbstractValidator<SubmitContactMessageRequest>
{
    public SubmitContactMessageRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

        RuleFor(x => x.Subject)
            .MaximumLength(300);

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required");
    }
}
