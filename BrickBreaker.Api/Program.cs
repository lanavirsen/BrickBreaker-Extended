using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using System.Text.Json.Serialization;
using BrickBreaker.Api;
using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Services;
using BrickBreaker.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

const string AuthLimiterPolicy = "auth-strict";

static string ResolveClientPartition(HttpContext context)
{
    if (!string.IsNullOrWhiteSpace(context.User?.Identity?.Name))
    {
        return $"user:{context.User.Identity!.Name!.Trim().ToLowerInvariant()}";
    }

    var ipAddress = context.Connection.RemoteIpAddress?.ToString();
    if (!string.IsNullOrWhiteSpace(ipAddress))
    {
        return $"ip:{ipAddress}";
    }

    return "anonymous";
}

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}
builder.Services.Configure<TurnstileOptions>(builder.Configuration.GetSection("Turnstile"));
builder.Services.AddHttpClient<ITurnstileVerifier, TurnstileVerifier>();
var allowedOrigins = builder.Configuration
                            .GetSection("Cors:AllowedOrigins")
                            .Get<string[]>()?
                            .Where(origin => !string.IsNullOrWhiteSpace(origin))
                            .Select(origin => origin.Trim().TrimEnd('/'))
                            .ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins is { Length: > 0 })
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
            return;
        }

        throw new InvalidOperationException(
            "CORS allowed origins are not configured. Set 'Cors:AllowedOrigins' before launching the API.");
    });
});

builder.Services.AddSingleton(sp =>
{
    var connectionString = StorageConfiguration.ReadFromEnvironment(builder.Environment.IsDevelopment())
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
builder.Services.AddSingleton<IProfanityFilter, DotnetProfanityFilter>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            ResolveClientPartition(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    options.AddPolicy(AuthLimiterPolicy, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            ResolveClientPartition(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 8,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
                 ?? throw new InvalidOperationException("JWT configuration missing.");
if (string.IsNullOrWhiteSpace(jwtOptions.Secret))
{
    throw new InvalidOperationException("JWT secret missing.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/register", async (RegisterRequest request, IAuthService auth, ITurnstileVerifier turnstile, HttpContext httpContext) =>
{
    var captchaFailure = await EnforceTurnstileAsync(request.TurnstileToken, turnstile, httpContext);
    if (captchaFailure is not null)
    {
        return captchaFailure;
    }

    // Service layer handles all validation and business rules (required fields, profanity, password strength)
    var result = await auth.RegisterAsync(request.Username, request.Password);
    if (!result.Success)
    {
        return result.ErrorCode switch
        {
            "username_required" => ValidationError("username_required", "Choose a username to continue."),
            "username_profane" => ValidationError("username_profane", "That username is not allowed. Try another one."),
            "username_taken" => ValidationError("username_taken", "That username is already taken. Try another one."),
            "password_too_short" => ValidationError("password_too_short", "Passwords must be at least 5 characters long."),
            _ => ValidationError("registration_failed", "Registration failed. Please try again.")
        };
    }

    return Results.Ok();
}).RequireRateLimiting(AuthLimiterPolicy);

app.MapPost("/login", async (LoginRequest request, IAuthService auth, IJwtTokenGenerator tokens, ITurnstileVerifier turnstile, HttpContext httpContext) =>
{
    var captchaFailure = await EnforceTurnstileAsync(request.TurnstileToken, turnstile, httpContext);
    if (captchaFailure is not null)
    {
        return captchaFailure;
    }

    var normalizedUsername = (request.Username ?? string.Empty).Trim();
    var password = request.Password ?? string.Empty;
    var success = await auth.LoginAsync(normalizedUsername, password);
    if (!success)
    {
        return ValidationError("invalid_credentials", "Invalid username or password.");
    }

    var token = tokens.GenerateToken(normalizedUsername);
    return Results.Ok(new LoginResponse(normalizedUsername, token));
}).RequireRateLimiting(AuthLimiterPolicy);

app.MapGet("/leaderboard/top", async (int count, ILeaderboardService leaderboard) =>
{
    var entries = await leaderboard.TopAsync(count);
    return Results.Ok(entries);
});

app.MapGet("/leaderboard/best/{username}", async (string username, ClaimsPrincipal user, ILeaderboardService leaderboard) =>
{
    if (!IsAuthorizedUser(user, username))
    {
        return Results.Forbid();
    }

    var canonical = user.Identity?.Name?.Trim() ?? username.Trim();
    var best = await leaderboard.BestForAsync(canonical);
    return best is null ? Results.NotFound() : Results.Ok(best);
}).RequireAuthorization();

app.MapPost("/leaderboard/submit", async (SubmitScoreRequest request, ClaimsPrincipal user, ILeaderboardService leaderboard) =>
{
    if (!IsAuthorizedUser(user, request.Username))
    {
        return Results.Forbid();
    }

    var canonical = user.Identity?.Name?.Trim() ?? request.Username.Trim();
    await leaderboard.SubmitAsync(canonical, request.Score);
    return Results.Ok();
}).RequireAuthorization();

app.Run();

static async Task<IResult?> EnforceTurnstileAsync(string? token, ITurnstileVerifier verifier, HttpContext context)
{
    var remoteIp = context.Connection.RemoteIpAddress?.ToString();
    var isHuman = await verifier.VerifyAsync(token, remoteIp, context.RequestAborted);
    return isHuman ? null : ValidationError("captcha_failed", "Complete the CAPTCHA to continue.");
}

static IResult ValidationError(string code, string message) =>
    Results.Json(new ApiError(code, message), statusCode: StatusCodes.Status400BadRequest);

static bool IsAuthorizedUser(ClaimsPrincipal user, string targetUsername)
{
    if (string.IsNullOrWhiteSpace(targetUsername))
    {
        return false;
    }

    var normalizedTarget = targetUsername.Trim();
    var principalName = user.Identity?.Name;
    return !string.IsNullOrWhiteSpace(principalName) &&
           string.Equals(principalName.Trim(), normalizedTarget, StringComparison.OrdinalIgnoreCase);
}

record RegisterRequest(string Username, string Password, string? TurnstileToken = null);
record LoginRequest(string Username, string Password, string? TurnstileToken = null);
record LoginResponse(string Username, string Token);
record SubmitScoreRequest(string Username, int Score);
record ApiError([property: JsonPropertyName("error")] string Error, [property: JsonPropertyName("message")] string Message);
