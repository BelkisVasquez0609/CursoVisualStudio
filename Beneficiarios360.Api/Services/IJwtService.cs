using Beneficiarios360.Api.DTOs;
using Beneficiarios360.Api.Entities;

namespace Beneficiarios360.Api.Services
{
    public interface IJwtService
    {
        LoginResponse GenerateToken(AppUser user);
    }
}
