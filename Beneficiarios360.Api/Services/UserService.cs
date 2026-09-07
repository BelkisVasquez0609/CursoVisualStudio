using Beneficiarios360.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace Beneficiarios360.Api.Services;

public sealed class UserService :
    IUserService
{
    private readonly PasswordHasher<AppUser>  _passwordHasher;

    private readonly List<AppUser> _users;

    public UserService(PasswordHasher<AppUser> passwordHasher)
    {
        _passwordHasher = passwordHasher;

        AppUser administrator =
            new()
            {
                Id = 1,
                UserName = "admin",
                FullName = "Administrador del sistema",
                Role = "Administrador",
                Active = true
            };

        administrator.PasswordHash = _passwordHasher.HashPassword(administrator, "Admin123*");

        AppUser consultationUser =
            new()
            {
                Id = 2,
                UserName = "consulta",
                FullName = "Usuario de consulta",
                Role = "Consulta",
                Active = true
            };

        consultationUser.PasswordHash = _passwordHasher.HashPassword(consultationUser, "Consulta123*");

        _users =
        [
            administrator,
            consultationUser
        ];
    }

    public AppUser? Authenticate(string userName, string password)
    {
        AppUser? user = _users.FirstOrDefault( item => item.Active && item.UserName.Equals(userName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user is null)
            return null;

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }
}