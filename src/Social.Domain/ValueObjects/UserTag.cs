using Social.Domain.Abstractions;
using Social.Domain.Constants;

namespace Social.Domain.ValueObjects;

public class UserTag : ValueObject
{
    public string Tag { get; private set; } = null!;

    public UserTag() { } // For EF Core

    private UserTag(string tag)
    {
        Tag = tag;
    }

    public static UserTag Create(string tag)
    {
        if (tag.Length != UserTagConstants.Length || !tag.All(char.IsAsciiDigit))
            throw new ArgumentException($"A user tag must be {UserTagConstants.Length} digits.", nameof(tag));

        return new UserTag(tag);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Tag;
    }
}
