using BrickBreaker.Core.Services;

namespace BrickBreaker.Tests;

public sealed class DotnetProfanityFilterTests
{
    [Fact]
    public void ContainsProfanity_ReturnsFalseForNullOrWhitespace()
    {
        var filter = new DotnetProfanityFilter();

        Assert.False(filter.ContainsProfanity(null));
        Assert.False(filter.ContainsProfanity("   "));
    }

    [Fact]
    public void ContainsProfanity_DetectsCommonProfanity()
    {
        var filter = new DotnetProfanityFilter();

        Assert.True(filter.ContainsProfanity("what the hell"));
        Assert.False(filter.ContainsProfanity("hello world"));
    }
}
