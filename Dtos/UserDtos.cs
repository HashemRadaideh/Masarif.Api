namespace Masarif.Api.Dtos;

public sealed record UserReadDto(
    int Id,
    string UserName,
    string Email,
    string Role,
    DateTime CreatedAt
);

public sealed record UserCreateDto(string UserName, string Email, string Password, string Role);

public sealed record UserUpdateDto(string UserName, string Email, string Role);

public sealed record SelfUpdateDto(string UserName, string Email, string? NewPassword);
