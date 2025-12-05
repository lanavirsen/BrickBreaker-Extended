using System;
using System.IO;
using BrickBreaker.Core.Clients;

namespace BrickBreaker.Tests.Clients;

public class ApiConfigurationTests
{
    [Fact]
    public void PreferredValueWins()
    {
        var value = ApiConfiguration.ResolveBaseAddress("http://custom-host:8080");
        Assert.Equal("http://custom-host:8080/", value);
    }

    [Fact]
    public void EnvironmentFallbackIsUsed()
    {
        const string envValue = "http://env-host/";
        var original = Environment.GetEnvironmentVariable(ApiConfiguration.BaseAddressEnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(ApiConfiguration.BaseAddressEnvironmentVariable, envValue);
            var result = ApiConfiguration.ResolveBaseAddress(settingsPath: null);
            Assert.Equal(envValue, result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ApiConfiguration.BaseAddressEnvironmentVariable, original);
        }
    }

    [Fact]
    public void JsonSettingsFallbackIsUsed()
    {
        using var temp = new TempJsonFile();
        File.WriteAllText(temp.Path, "{ \"ApiBaseUrl\": \"http://file-host\" }");
        var result = ApiConfiguration.ResolveBaseAddress(settingsPath: temp.Path);
        Assert.Equal("http://file-host/", result);
    }
}
