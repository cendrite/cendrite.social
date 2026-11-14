using FluentValidation;

namespace Social.Application.Features.UserProfile.Commands.Delete.Avatar;

public sealed class DeleteAvatarCommandValidator : AbstractValidator<DeleteAvatarCommand>
{
    public DeleteAvatarCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
    }
}
