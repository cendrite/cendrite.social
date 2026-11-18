using Social.Domain.Abstractions;
using Social.Domain.Common;
using Social.Domain.Constants;
using Social.Domain.Entities;
using Social.Domain.Enums;
using Social.Domain.Events.UserProfile;
using Social.Domain.ValueObjects;

namespace Social.Domain.Aggregates;

public partial class UserProfile : AggregateRoot
{
    public string Username { get; private set; } = null!;
    public UserTag UserTag { get; private set; } = null!;
    public string? DisplayName { get; private set; }
    public bool IsPrivate { get; private set; }
    private Avatar? _avatar;
    public Avatar? Avatar => _avatar;

    private readonly List<BlockedUser> _blockedUsers = [];
    public IReadOnlyCollection<BlockedUser> BlockedUsers => _blockedUsers;

    private readonly List<Friendship> _friendshipsReceived = [];
    public IReadOnlyCollection<Friendship> FriendshipsReceived => _friendshipsReceived;

    private readonly List<Friendship> _friendshipsInitiated = [];
    public IReadOnlyCollection<Friendship> FriendshipsInitiated => _friendshipsInitiated;

    public UserProfile() : base(Guid.Empty) { } // For EF Core

    public UserProfile(Guid id, string username, UserTag userUserTag) : base(id)
    {
        Username = username;
        DisplayName = username;
        UserTag = userUserTag;
    }

    public Avatar GetOrCreateAvatar(string contentType)
    {
        if (_avatar is null)
        {
            _avatar = new Avatar(Id, Guid.NewGuid(), contentType, ETag.Generate());
            AddDomainEvent(new AvatarUpdatedEvent(Id));
            return _avatar;
        }

        _avatar.UpdateContentType(contentType);
        _avatar.BumpTag();
        AddDomainEvent(new AvatarUpdatedEvent(Id));
        return _avatar;
    }

    public Avatar? RemoveAvatar()
    {
        if (_avatar is null) return null;

        var removed = _avatar;
        _avatar = null;
        AddDomainEvent(new AvatarUpdatedEvent(Id));
        return removed;
    }

    public static Result ValidateName(string name)
    {
        if (name.Length < ProfileNameConstants.MinLength)
            return Result.Fail(Error.Validation($"Value '{nameof(name)}' should be at least {ProfileNameConstants.MinLength}."));
        if (name.Length > ProfileNameConstants.MaxLength)
            return Result.Fail(Error.Validation($"Value '{nameof(name)}' should not exceed {ProfileNameConstants.MaxLength}."));
        if (!NameRegex().IsMatch(name))
            return Result.Fail(Error.Validation($"Value '{nameof(name)}' may only contain letters and digits"));
        if (IsReserved(name))
            return Result.Fail(Error.Validation($"Value '{nameof(name)}' contains a reserved name"));

        return Result.Ok();
    }

    public Result SetDisplayName(string displayName)
    {
        var valid = ValidateName(displayName);
        if (!valid.Success) return valid;

        DisplayName = displayName;
        AddDomainEvent(new UserProfileDisplayNameUpdatedEvent(Id));
        return Result.Ok();
    }

    public void TogglePrivacy(bool isPrivate) => IsPrivate = isPrivate;

    public Result AddFriendship(UserProfile userProfile)
    {
        if (Id == userProfile.Id)
            return Result.Fail(Error.BadRequest("friendship.self", "Cannot add self as friend"));

        if (FriendshipsInitiated.Any(f => f.FriendProfileId == userProfile.Id) || FriendshipsReceived.Any(f => f.UserProfileId == userProfile.Id))
            return Result.Fail(Error.BadRequest("friendship.exists", "Friendship already exists"));

        _friendshipsInitiated.Add(new(Id, userProfile.Id));
        return Result.Ok();
    }

    //TODO: should probably not return Result, but just throw exceptions
    public Result<Guid> DeleteFriendship(string friendTag)
    {
        if (friendTag == UserTag.Tag)
            throw new InvalidOperationException("Cannot delete self as friend");

        var friendshipInitiated = FriendshipsInitiated.FirstOrDefault(f => f.FriendProfile.UserTag.Tag == friendTag);

        if (friendshipInitiated is not null)
        {
            _friendshipsInitiated.Remove(friendshipInitiated);
            return Result.Ok(friendshipInitiated.FriendProfileId);
        }

        var friendshipReceived = FriendshipsReceived.FirstOrDefault(f => f.UserProfile.UserTag.Tag == friendTag);

        if (friendshipReceived is not null)
        {
            _friendshipsReceived.Remove(friendshipReceived);
            return Result.Ok(friendshipReceived.UserProfileId);
        }

        return Result.Fail<Guid>(Error.BadRequest("friendship.not.found", "Friendship not found"));
    }

    public IReadOnlyCollection<UserProfile> GetFriends()
    {
        var friends = new List<UserProfile>();

        friends.AddRange(FriendshipsInitiated
            .Where(x => x.Status == FriendshipStatus.Accepted)
            .Select(f => f.FriendProfile));

        friends.AddRange(FriendshipsReceived
            .Where(x => x.Status == FriendshipStatus.Accepted)
            .Select(f => f.UserProfile));

        return friends;
    }

    public void BlockUser(Guid blockedUserId)
    {
        if (blockedUserId == Guid.Empty)
            throw new InvalidOperationException("BlockedUserId cannot be empty");

        if (blockedUserId == Id)
            throw new InvalidOperationException("Cannot block self");

        if (_blockedUsers.Any(c => c.BlockedUserProfileId == blockedUserId))
            throw new InvalidOperationException("User already blocked");

        _blockedUsers.Add(new BlockedUser(blockingUserProfileId: Id, blockedUserProfileId: blockedUserId));
        AddDomainEvent(new UserBlockedEvent(Id, blockedUserId));
    }

    public void UnblockUser(Guid unblockedUserId)
    {
        if (unblockedUserId == Guid.Empty)
            throw new InvalidOperationException("UnblockedUserId cannot be empty");

        if (unblockedUserId == Id)
            throw new InvalidOperationException("Cannot unblock self");


        var blockedUser = BlockedUsers.FirstOrDefault(c => c.BlockedUserProfileId == unblockedUserId);
        if (blockedUser is null)
            throw new InvalidOperationException("User not blocked");

        _blockedUsers.Remove(blockedUser);
    }


    private static bool IsReserved(string name)
    {
        var lower = name.ToLowerInvariant();

        return ReservedNameConstants.Words.Any(word =>
            Enumerable.Range(0, Math.Max(0, lower.Length - word.Length + 1)).Any(start =>
                word.Select((letter, i) => Reads(lower[start + i], letter)).All(match => match)));
    }

    private static bool Reads(char c, char letter) =>
        c == letter || (ReservedNameConstants.LookAlikes.TryGetValue(c, out var lookAlikes) && lookAlikes.Contains(letter));

    [System.Text.RegularExpressions.GeneratedRegex(@"^[a-zA-Z0-9]*$")]
    private static partial System.Text.RegularExpressions.Regex NameRegex();
}
