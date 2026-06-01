using RazorClassLibrary.Services;
using Xunit;

namespace TestProjectxUnit
{
    public class AccessKeyMappingTests
    {
        [Theory]
        [InlineData(0, "0")]
        [InlineData(9, "9")]
        [InlineData(10, "a")]
        [InlineData(11, "b")]
        [InlineData(28, "z")]
        public void GetKeyForIndex_ReturnsExpected(int index, string expected)
        {
            var key = AccessKeyMapper.GetKeyForIndex(index);
            Assert.Equal(expected, key);
        }
    }
}
