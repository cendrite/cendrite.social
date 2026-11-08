using FluentAssertions;
using Social.Domain.Services;
using Xunit;
using Social.Test.Unit.Setup.Helpers;

namespace Social.Test.Unit.ServicesTest
{
    public class UserProfileLifecycleLifecycleServiceTest
    {
        private readonly IUserProfileLifecycleService _sut = new UserProfileLifecycleLifecycleService();

        [Fact]
        public void CreateUserProfile_ShouldReturnValidUserProfile_WhenCalledWithValidInput()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var username = "TestUser";

            // Act
            var userProfile = _sut.CreateUserProfile(userId, username, UserTags.New());

            // Assert
            userProfile.Should().NotBeNull();
            userProfile.Id.Should().Be(userId);
            userProfile.Username.Should().Be(username);
        }

        [Fact]
        public void CreateUserProfile_ShouldUseTheTagFromIdentity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userTag = UserTags.New();

            // Act
            var userProfile = _sut.CreateUserProfile(userId, "AnotherUser", userTag);

            // Assert
            userProfile.UserTag.Tag.Should().Be(userTag);
        }
    }
}
