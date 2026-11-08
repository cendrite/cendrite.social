using Social.Domain.Aggregates;
using Social.Domain.ValueObjects;

namespace Social.Domain.Services;

public interface IUserProfileLifecycleService
{
    UserProfile CreateUserProfile(Guid userId, string username, string userTag);
    void SoftDelete(UserProfile userProfile);
    void RevertSoftDelete(UserProfile userProfile);
}
public class UserProfileLifecycleLifecycleService : IUserProfileLifecycleService
{
    public UserProfile CreateUserProfile(Guid userId, string username, string userTag)
    {
        return new(userId, username, UserTag.Create(userTag));
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
