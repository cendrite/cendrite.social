using System;
using System.Threading;
using System.Threading.Tasks;
using Social.Infrastructure.Persistence.Context;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Social.Application.Contracts.Repositories;
using Social.Application.Features.Friendships.Commands.Delete.Friendship;
using Social.Domain.Aggregates;
using Social.Domain.Interfaces;
using Social.Domain.ValueObjects;
using Social.Test.Integration.Setup;
using Xunit;
using Social.Test.Integration.Setup.Helpers;

namespace Social.Test.Integration.UserProfileTest.CommandTest.DeleteTest.Friendship;

public class DeleteFriendshipCommandHandlerTest : IntegrationTestBase
{
    private readonly DeleteFriendshipCommandHandler _sut;
    public DeleteFriendshipCommandHandlerTest(IntegrationTestFactory<Program, SocialDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUserProfileRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var userProfileService = scope.ServiceProvider.GetRequiredService<IFriendshipService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DeleteFriendshipCommandHandler>>();
        _sut = new(repo, unitOfWork, userProfileService, logger);
    }

    [Fact]
    public async Task Handle_Given_Valid_Command_Should_Delete_Friendship()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), "TestUser", UserTag.Create(UserTags.New()));

        var friendProfile = new UserProfile(Guid.NewGuid(), "FriendUser", UserTag.Create(UserTags.New()));

        userProfile.AddFriendship(friendProfile);
        Db.UserProfile.Add(userProfile);
        Db.UserProfile.Add(friendProfile);
        await Db.SaveChangesAsync();
        Db.Friendship.Should().HaveCount(1);

        var cmd = new DeleteFriendshipCommand()
        {
            TenantId = userProfile.Id,
            FriendTag = friendProfile.UserTag.Tag
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
        Db.Friendship.Should().HaveCount(0);
        Db.OutboxMessage.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Handle_Given_Several_Initiated_Friendships_Should_Delete_Only_The_Named_One(int target)
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), "TestUser", UserTag.Create(UserTags.New()));
        var friends = new[]
        {
            new UserProfile(Guid.NewGuid(), "FriendOne", UserTag.Create(UserTags.New())),
            new UserProfile(Guid.NewGuid(), "FriendTwo", UserTag.Create(UserTags.New())),
        };

        foreach (var friend in friends) userProfile.AddFriendship(friend);
        Db.UserProfile.Add(userProfile);
        Db.UserProfile.AddRange(friends);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var cmd = new DeleteFriendshipCommand()
        {
            TenantId = userProfile.Id,
            FriendTag = friends[target].UserTag.Tag
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        Db.ChangeTracker.Clear();
        Db.Friendship.Should().ContainSingle()
            .Which.FriendProfileId.Should().Be(friends[1 - target].Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Handle_Given_Several_Received_Friendships_Should_Delete_Only_The_Named_One(int target)
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), "TestUser", UserTag.Create(UserTags.New()));
        var friends = new[]
        {
            new UserProfile(Guid.NewGuid(), "FriendOne", UserTag.Create(UserTags.New())),
            new UserProfile(Guid.NewGuid(), "FriendTwo", UserTag.Create(UserTags.New())),
        };

        foreach (var friend in friends) friend.AddFriendship(userProfile);
        Db.UserProfile.Add(userProfile);
        Db.UserProfile.AddRange(friends);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var cmd = new DeleteFriendshipCommand()
        {
            TenantId = userProfile.Id,
            FriendTag = friends[target].UserTag.Tag
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        Db.ChangeTracker.Clear();
        Db.Friendship.Should().ContainSingle()
            .Which.UserProfileId.Should().Be(friends[1 - target].Id);
    }

    [Fact]
    public async Task Handle_Given_Command_With_Invalid_UserProfileId_Should_Return_NotFound()
    {
        // Arrange
        var cmd = new DeleteFriendshipCommand()
        {
            TenantId = Guid.NewGuid(),
            FriendTag = "FriendUser"
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error!.Code.Should().Be("entity.not.found");
    }

    [Fact]
    public async Task Handle_Given_Command_With_No_Friendship_Should_Return_Result_Fail()
    {
        // Arrange
        var userProfile = new UserProfile(Guid.NewGuid(), "TestUser", UserTag.Create(UserTags.New()));

        var friendProfile = new UserProfile(Guid.NewGuid(), "FriendUser", UserTag.Create(UserTags.New()));

        //NOTE: No friendship added
        Db.UserProfile.Add(userProfile);
        Db.UserProfile.Add(friendProfile);
        await Db.SaveChangesAsync();

        var cmd = new DeleteFriendshipCommand()
        {
            TenantId = userProfile.Id,
            FriendTag = friendProfile.UserTag.Tag
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Description.Should().Be("Friendship not found");
        Db.Friendship.Should().HaveCount(0);
    }
}