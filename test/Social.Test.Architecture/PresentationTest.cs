using Social.Test.Architecture.Helpers;
using NetArchTest.Rules;
using Social.API.Controllers;

namespace Social.Test.Architecture;

public class PresentationTest
{
    [Fact]
    public void Presentation_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(UserProfileController).Assembly)
            .That()
            .ResideInNamespace("Cendrite.Authentication.Controllers")
            .ShouldNot()
            .HaveDependencyOn("Cendrite.Identity.Persistence")
            .GetResult();

        result.ShouldBeSuccessful("API project should not reference Infrastructure project");
    }

    [Fact]
    public void Presentation_Should_Not_Reference_Domain()
    {
        var result = Types.InAssembly(typeof(UserProfileController).Assembly)
            .That()
            .ResideInNamespace("Cendrite.ChatUser.API.ControllerTest")
            .ShouldNot()
            .HaveDependencyOn("Cendrite.ChatUser.Domain")
            .GetResult();

        result.ShouldBeSuccessful("API project should not reference Domain project");
    }

    [Fact]
    public void Presentation_Should_Reference_Application()
    {
        var result = Types.InAssembly(typeof(UserProfileController).Assembly)
            .That()
            .ResideInNamespace("Cendrite.ChatUser.API.ControllerTest")
            .And()
            .DoNotHaveName("BaseController")
            .Should()
            .HaveDependencyOn("Cendrite.ChatUser.Application")
            .GetResult();

        result.ShouldBeSuccessful("API project should reference Application project");
    }
}
