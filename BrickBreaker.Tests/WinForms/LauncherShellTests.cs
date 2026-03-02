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
    // Quick-play mode should be active by default so the game is playable
    // without requiring an account.
    [Fact]
    public void CanStartGameByDefaultWithoutLoggingIn()
    {
        using var shell = CreateShell();
        var view = shell.Snapshot();
        Assert.True(view.CanStartGame);
        Assert.True(view.IsQuickPlay);
    }

    // Full happy-path flow: log in, finish a game, submit the score.
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

    // A successful leaderboard fetch should update the status message.
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

    // Builds a LauncherShell wired to a stub HTTP handler so no real network
    // calls are made during tests.
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

    // Routes stub requests to canned responses based on the URL path.
    private static HttpResponseMessage BuildResponse(HttpRequestMessage request)
    {
        var path = request.RequestUri!.AbsolutePath.Trim('/');
        return path switch
        {
            "login" => Json(new { username = "tester", token = "fake-token" }),
            "register" => Success(),
            "leaderboard/submit" => Success(),
            var p when p.StartsWith("leaderboard/top", StringComparison.OrdinalIgnoreCase)
                => Json(new[] { new ScoreEntry("alice", 50, DateTimeOffset.UtcNow) }),
            _ => Success()
        };
    }

    // Minimal HttpMessageHandler that delegates to a swappable factory function.
    private sealed class StubHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ResponseFactory is null)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

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
