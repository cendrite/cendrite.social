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
            var userProfile = _sut.CreateUserProfile(userId, username, UserTags.New()).RequiredValue;

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
            var userProfile = _sut.CreateUserProfile(userId, "AnotherUser", userTag).RequiredValue;

            // Assert
            userProfile.UserTag.Tag.Should().Be(userTag);
        }

        [Theory]
        [InlineData("admin")]
        [InlineData("Cendrite")]
        [InlineData("c3ndr1te")]
        [InlineData("a")]
        [InlineData("has space")]
        [InlineData("аdmin")]
        public void CreateUserProfile_ShouldFail_WhenUsernameIsNotAllowed(string username)
        {
            // Act
            var result = _sut.CreateUserProfile(Guid.NewGuid(), username, UserTags.New());

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        public void CreateUserProfile_ShouldAcceptATwoLetterUsername()
        {
            // Act
            var result = _sut.CreateUserProfile(Guid.NewGuid(), "Bo", UserTags.New());

            // Assert
            result.Success.Should().BeTrue();
        }
    }
}
