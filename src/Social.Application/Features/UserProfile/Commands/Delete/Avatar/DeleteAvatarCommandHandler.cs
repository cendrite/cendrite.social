using Social.Application.Abstractions;
using Social.Application.Contracts.Repositories;
using Social.Application.Contracts.Services;
using Social.Domain.Common;

namespace Social.Application.Features.UserProfile.Commands.Delete.Avatar;

public sealed class DeleteAvatarCommandHandler(
    IUserProfileRepository userProfileRepository,
    IUnitOfWork unitOfWork,
    IAvatarService avatarService)
    : ICommandHandler<DeleteAvatarCommand>
{
    public async ValueTask<Result> Handle(DeleteAvatarCommand cmd, CancellationToken ct)
    {
        var userProfile = await userProfileRepository.GetSingleAsync(new UserProfileWithAvatarSpec(cmd.TenantId), ct);
        if (userProfile is null)
            return Result.Fail(Error.NotFound<Domain.Aggregates.UserProfile>(cmd.TenantId.ToString()));

        var removed = userProfile.RemoveAvatar();
        if (removed is null) return Result.Ok();

        await unitOfWork.SaveChangesAsync(ct);

        avatarService.Delete(removed.FileKey);

        return Result.Ok();
    }
}
