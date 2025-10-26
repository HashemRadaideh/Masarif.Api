using Masarif.Api.Dtos;
using Masarif.Api.Models;

namespace Masarif.Api.Mapping;

public static class UserMapping
{
    public static UserReadDto ToReadDto(this AppUser u) =>
        new(u.Id, u.UserName, u.Email, u.Role, u.CreatedAt);

    public static void Apply(this AppUser u, UserUpdateDto d)
    {
        u.UserName = d.UserName;
        u.Email = d.Email;
        u.Role = d.Role;
    }

    public static void ApplySelf(this AppUser u, SelfUpdateDto d, string? newPasswordHash = null)
    {
        u.UserName = d.UserName;
        u.Email = d.Email;
        if (!string.IsNullOrWhiteSpace(newPasswordHash))
            u.PasswordHash = newPasswordHash;
    }
}
