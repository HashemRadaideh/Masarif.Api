namespace Masarif.Api.Dtos;

public sealed record SignupRequest(string UserName, string Email, string Password);

public sealed record LoginRequest(string UserNameOrEmail, string Password);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, string Role);

public sealed record LogoutResponse(bool Success);
