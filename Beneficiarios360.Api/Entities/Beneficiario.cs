namespace Beneficiarios360.Api.Entities
{
    public sealed class Beneficiario
    {
        public int Id { get; set; }

        public required string Documento { get; set; }

        public required string Nombres { get; set; }

        public required string Apellidos { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime CreadoUtc { get; set; } =
            DateTime.UtcNow;
    }
}
