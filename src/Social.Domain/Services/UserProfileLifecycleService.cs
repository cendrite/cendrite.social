using Social.Domain.Aggregates;
using Social.Domain.Common;
using Social.Domain.ValueObjects;

namespace Social.Domain.Services;

public interface IUserProfileLifecycleService
{
    Result<UserProfile> CreateUserProfile(Guid userId, string username, string userTag);
    void SoftDelete(UserProfile userProfile);
    void RevertSoftDelete(UserProfile userProfile);
}
public class UserProfileLifecycleLifecycleService : IUserProfileLifecycleService
{
    public Result<UserProfile> CreateUserProfile(Guid userId, string username, string userTag)
    {
        var valid = UserProfile.ValidateName(username);
        if (!valid.Success) return Result.Fail<UserProfile>(valid.Error);

        return Result.Ok(new UserProfile(userId, username, UserTag.Create(userTag)));
    }

    public void SoftDelete(UserProfile userProfile)
    {
        userProfile.SetDelete();
    }

    public void RevertSoftDelete(UserProfile userProfile)
    {
        userProfile.RevertDelete();
    }
}
