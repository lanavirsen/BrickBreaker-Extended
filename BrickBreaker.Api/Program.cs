using BrickBreaker.Api;
using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Services;
using BrickBreaker.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();

record RegisterRequest(string Username, string Password);
record LoginRequest(string Username, string Password);
