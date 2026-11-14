using Social.Application.Abstractions;

namespace Social.Application.Features.UserProfile.Commands.Delete.Avatar;

public sealed record DeleteAvatarCommand(Guid TenantId) : ICommand;
