using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Social.Application.Contracts.Repositories;
using Social.Application.Contracts.Services;
using Social.Application.Features.UserProfile.Commands.Delete.Avatar;
using Social.Application.Features.UserProfile.Commands.Update.Avatar;
using Social.Domain.Aggregates;
using Social.Domain.ValueObjects;
using Social.Infrastructure.Persistence.Context;
using Social.Test.Integration.Setup;
using Social.Test.Integration.Setup.Helpers;
using Xunit;

namespace Social.Test.Integration.UserProfileTest.CommandTest.DeleteTest.Avatar;

public class DeleteAvatarCommandHandlerTest : IntegrationTestBase
{
    private readonly DeleteAvatarCommandHandler _sut;
    private readonly UpdateAvatarCommandHandler _upload;
    private readonly IAvatarService _avatarService;

    public DeleteAvatarCommandHandlerTest(IntegrationTestFactory<Program, SocialDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUserProfileRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        _avatarService = scope.ServiceProvider.GetRequiredService<IAvatarService>();

        _sut = new DeleteAvatarCommandHandler(repo, unitOfWork, _avatarService);
        _upload = new UpdateAvatarCommandHandler(repo, unitOfWork, _avatarService);
    }

    private async Task<UserProfile> UserWithAvatar()
    {
        var user = new UserProfile(Guid.NewGuid(), "TestUser", UserTag.Create(UserTags.New()));
        Db.UserProfile.Add(user);
        await Db.SaveChangesAsync();

        await _upload.Handle(new UpdateAvatarCommand
        {
            TenantId = user.Id,
            Avatar = FormFileHelper.CreateFormFile(Path.Combine(DirectoryHelper.GetProjectRootDirectory(), "test/Social.Test.Integration/Setup/Resources/test_profile_picture.png"), "image/png")
        }, CancellationToken.None);

        return user;
    }

    [Fact]
    public async Task Handle_Given_User_With_Avatar_Should_Remove_The_Avatar_And_Its_File()
    {
        // Arrange
        var user = await UserWithAvatar();
        var fileKey = (await Db.Avatar.AsNoTracking().SingleAsync(a => a.UserProfileId == user.Id)).FileKey;

        // Act
        var result = await _sut.Handle(new DeleteAvatarCommand(user.Id), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        (await Db.Avatar.AsNoTracking().AnyAsync(a => a.UserProfileId == user.Id)).Should().BeFalse();
        var open = () => _avatarService.Get(fileKey);
        open.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public async Task Handle_Given_User_Without_Avatar_Should_Succeed()
    {
        // Arrange
        var user = new UserProfile(Guid.NewGuid(), "TestUser", UserTag.Create(UserTags.New()));
        Db.UserProfile.Add(user);
        await Db.SaveChangesAsync();

        // Act
        var result = await _sut.Handle(new DeleteAvatarCommand(user.Id), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Given_Unknown_User_Should_Return_Not_Found()
    {
        // Act
        var result = await _sut.Handle(new DeleteAvatarCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
    }
}
