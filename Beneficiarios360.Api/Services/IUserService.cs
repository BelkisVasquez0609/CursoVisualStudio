using Beneficiarios360.Api.Entities;

namespace Beneficiarios360.Api.Services
{
    public interface IUserService
    {
        AppUser? Authenticate(string userName, string password);
    }
}
