using VAuto.Zone.Core;
using Xunit;

namespace Bluelock.Tests
{
    public class LifecycleActionTokenTests
    {
        [Theory]
        [InlineData("boss_enter", "boss_enter", "")]
        [InlineData("boss-enter", "boss_enter", "")]
        [InlineData("bossenter", "boss_enter", "")]
        [InlineData("boss_enter:arena_default", "boss_enter", "arena_default")]
        [InlineData("clear_template:boss", "clear_template", "boss")]
        [InlineData("cleartemplate:boss", "clear_template", "boss")]
        public void TryParse_ParsesActionAndOptionalParameter(string raw, string expectedAction, string expectedParameter)
        {
            var ok = LifecycleActionToken.TryParse(raw, out var action, out var parameter);

            Assert.True(ok);
            Assert.Equal(expectedAction, action);
            Assert.Equal(expectedParameter, parameter);
        }

        [Fact]
        public void TryParse_RejectsEmptyTokens()
        {
            var ok = LifecycleActionToken.TryParse("   ", out var action, out var parameter);

            Assert.False(ok);
            Assert.Equal(string.Empty, action);
            Assert.Equal(string.Empty, parameter);
        }
    }
}
