namespace Sensores.Api.Models
{
    public class SensorData
    {
        public Guid Id { get; set; }

        public string ClientId { get; set; } = string.Empty;

        public double Temperatura { get; set; }

        public double Humedad { get; set; }

        public DateTime FechaEnvio { get; set; }

        public DateTime FechaRecepcion { get; set; } = DateTime.UtcNow;

        public string? IpDispositivo { get; set; }

        public long IpDispositivoNumero { get; set; }

        public string? IpOrigen { get; set; }

        public long IpOrigenNumero { get; set; }

        public string? IpContenedor { get; set; }

        public string? JsonOriginal { get; set; }
    }
}