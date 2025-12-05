using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BrickBreaker.Core.Clients;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Tests.WinForms;

public class LauncherShellTests
{
    [Fact]
    public void EnableQuickPlayUpdatesState()
    {
        using var shell = CreateShell();
        shell.EnableQuickPlay();
        var view = shell.Snapshot();
        Assert.True(view.IsQuickPlay);
        Assert.Contains("Quick Play", view.PlayerLabel);
    }

    [Fact]
    public async Task LoginAndSubmitScoreFlow()
    {
        using var shell = CreateShell();
        var login = await shell.LoginAsync("tester", "secret");
        Assert.True(login.Success);
        var state = shell.Snapshot();
        Assert.Contains("tester", state.PlayerLabel);

        shell.RecordGameFinished(321);
        var submit = await shell.SubmitScoreAsync(321);
        Assert.True(submit.Success);
    }

    [Fact]
    public async Task LoadLeaderboardPopulatesStatus()
    {
        using var shell = CreateShell();
        var result = await shell.LoadLeaderboardAsync(10);
        Assert.True(result.Success);
        Assert.NotEmpty(result.Value!);
        var state = shell.Snapshot();
        Assert.Contains("Leaderboard updated", state.StatusMessage);
    }

    private static LauncherShell CreateShell()
    {
        var handler = new StubHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5050/")
        };
        var gameClient = new GameApiClient(httpClient.BaseAddress!.ToString(), httpClient);
        handler.ResponseFactory = request => BuildResponse(request);
        return new LauncherShell(gameClient);
    }

    private static HttpResponseMessage BuildResponse(HttpRequestMessage request)
    {
        var path = request.RequestUri!.AbsolutePath.Trim('/');
        return path switch
        {
            "login" => Success(),
            "register" => Success(),
            "leaderboard/submit" => Success(),
            var p when p.StartsWith("leaderboard/top", StringComparison.OrdinalIgnoreCase)
                => Json(new[] { new ScoreEntry("alice", 50, DateTimeOffset.UtcNow) }),
            _ => Success()
        };
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ResponseFactory is null)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }

            return Task.FromResult(ResponseFactory(request));
        }
    }

    private static HttpResponseMessage Success()
        => new(HttpStatusCode.OK) { Content = new StringContent(string.Empty) };

    private static HttpResponseMessage Json(object payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }
}
