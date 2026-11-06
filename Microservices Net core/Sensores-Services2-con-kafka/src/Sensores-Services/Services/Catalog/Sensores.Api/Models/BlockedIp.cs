namespace Sensores.Api.Models.Datos
{
    public class BlockedIp
    {
        public Guid Id { get; set; }

        public string Ip { get; set; } = string.Empty;

        public long IpNumero { get; set; }

        public bool IsBlocked { get; set; } = true;

        public string? Motivo { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}