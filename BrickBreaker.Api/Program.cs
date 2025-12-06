using BrickBreaker.Api;
using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Services;
using BrickBreaker.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton(sp =>
{
    var connectionString = StorageConfiguration.ReadFromEnvironment()
                           ?? throw new InvalidOperationException("Supabase connection string missing.");
    return new StorageConnectionFactory(connectionString);
});
builder.Services.AddSingleton<IUserStore>(sp =>
{
    var factory = sp.GetRequiredService<StorageConnectionFactory>();
    return new UserStore(factory.ConnectionString);
});
builder.Services.AddSingleton<ILeaderboardStore>(sp =>
{
    var factory = sp.GetRequiredService<StorageConnectionFactory>();
    return new LeaderboardStore(factory.ConnectionString);
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/register", async (RegisterRequest request, IAuthService auth) =>
{
    var success = await auth.RegisterAsync(request.Username, request.Password);
    return success ? Results.Ok() : Results.BadRequest();
});

app.MapPost("/login", async (LoginRequest request, IAuthService auth) =>
{
    var success = await auth.LoginAsync(request.Username, request.Password);
    return success ? Results.Ok() : Results.Unauthorized();
});

app.MapGet("/leaderboard/top", async (int count, ILeaderboardService leaderboard) =>
{
    var entries = await leaderboard.TopAsync(count);
    return Results.Ok(entries);
});

app.MapGet("/leaderboard/best/{username}", async (string username, ILeaderboardService leaderboard) =>
{
    var best = await leaderboard.BestForAsync(username);
    return best is null ? Results.NotFound() : Results.Ok(best);
});

app.MapPost("/leaderboard/submit", async (SubmitScoreRequest request, ILeaderboardService leaderboard) =>
{
    await leaderboard.SubmitAsync(request.Username, request.Score);
    return Results.Ok();
});

app.Run();

record RegisterRequest(string Username, string Password);
record LoginRequest(string Username, string Password);
record SubmitScoreRequest(string Username, int Score);
