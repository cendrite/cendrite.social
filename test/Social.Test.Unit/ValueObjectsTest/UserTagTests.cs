using FluentAssertions;
using Social.Domain.ValueObjects;
using Xunit;

namespace Social.Test.Unit.ValueObjectsTest
{
    public class UserTagTests
    {
        [Fact]
        public void Create_Given_Twelve_Digits_Should_Keep_Them()
        {
            UserTag.Create("482177301593").Tag.Should().Be("482177301593");
        }

        [Theory]
        [InlineData("")]
        [InlineData("48217730159")]
        [InlineData("4821773015930")]
        [InlineData("4821 7730 159")]
        [InlineData("alice#482177K")]
        public void Create_Given_Anything_But_Twelve_Digits_Should_Throw(string tag)
        {
            var act = () => UserTag.Create(tag);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Tags_With_The_Same_Digits_Should_Be_Equal()
        {
            UserTag.Create("482177301593").Should().Be(UserTag.Create("482177301593"));
            UserTag.Create("482177301593").Should().NotBe(UserTag.Create("000000000001"));
        }
    }
}
