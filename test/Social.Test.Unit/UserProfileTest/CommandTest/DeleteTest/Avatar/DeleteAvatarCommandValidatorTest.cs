using FluentAssertions;
using Social.Application.Features.UserProfile.Commands.Delete.Avatar;
using Xunit;

namespace Social.Test.Unit.UserProfileTest.CommandTest.DeleteTest.Avatar;

public class DeleteAvatarCommandValidatorTest
{
    private readonly DeleteAvatarCommandValidator _sut = new();

    [Fact]
    public void Validate_Given_Empty_TenantId_Should_Fail()
    {
        // Act
        var result = _sut.Validate(new DeleteAvatarCommand(Guid.Empty));

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Given_TenantId_Should_Pass()
    {
        // Act
        var result = _sut.Validate(new DeleteAvatarCommand(Guid.NewGuid()));

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
