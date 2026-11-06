namespace Sensores.Api.Models
{
    public class Device
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; } = default!;
        public string? Ip { get; set; }
        public bool IsBlocked { get; set; } = false;
        public DateTime LastSeen { get; set; }
    }
}
