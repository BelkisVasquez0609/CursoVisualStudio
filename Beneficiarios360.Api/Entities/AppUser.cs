namespace Beneficiarios360.Api.Entities
{
    public class AppUser
    {
        public int Id { get; init; }

        public string UserName { get; init; } =string.Empty;

        public string FullName { get; init; } =string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; init; } = string.Empty;

        public bool Active { get; init; }
    }
}
