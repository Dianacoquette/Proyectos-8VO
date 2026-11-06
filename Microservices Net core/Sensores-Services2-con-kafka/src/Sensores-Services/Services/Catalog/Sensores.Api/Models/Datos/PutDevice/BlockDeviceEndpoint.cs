using Carter;
using Marten;
using Sensores.Api.Models.Datos;
using Sensores.Api.Utils;

namespace Sensores.Api.Models.Datos.PutDevice
{
    public class BlockRequest
    {
        public bool IsBlocked { get; set; }
    }

    public class BlockIpRequest
    {
        public string Ip { get; set; } = string.Empty;

        public bool IsBlocked { get; set; }

        public string? Motivo { get; set; }
    }

    public class BlockDeviceEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/devices/block/{clientId}", async (
                string clientId,
                BlockRequest request,
                IDocumentSession session) =>
            {
                var device = await session.Query<Device>()
                    .FirstOrDefaultAsync(d => d.ClientId == clientId);

                if (device == null)
                {
                    return Results.NotFound(new
                    {
                        message = "Dispositivo no encontrado"
                    });
                }

                device.IsBlocked = request.IsBlocked;

                await session.SaveChangesAsync();

                return Results.Ok(new
                {
                    message = request.IsBlocked
                        ? "Dispositivo bloqueado correctamente"
                        : "Dispositivo desbloqueado correctamente",
                    device.ClientId,
                    device.IsBlocked
                });
            })
            .WithName("BlockDevice")
            .WithSummary("Bloquear o desbloquear dispositivo")
            .WithDescription("Cambia el estado de bloqueo de un dispositivo por ClientId.");

            app.MapPut("/devices/block-ip", async (
                BlockIpRequest request,
                IDocumentSession session) =>
            {
                if (string.IsNullOrWhiteSpace(request.Ip))
                {
                    return Results.BadRequest(new
                    {
                        message = "La IP es obligatoria"
                    });
                }

                var ip = request.Ip.Trim();

                var ipNumero = IpHelper.ConvertIpToNumber(ip);

                if (ipNumero == 0)
                {
                    return Results.BadRequest(new
                    {
                        message = "La IP no tiene un formato válido"
                    });
                }

                var blockedIp = await session.Query<BlockedIp>()
                    .FirstOrDefaultAsync(x => x.IpNumero == ipNumero);

                if (blockedIp == null)
                {
                    blockedIp = new BlockedIp
                    {
                        Id = Guid.NewGuid(),
                        Ip = ip,
                        IpNumero = ipNumero,
                        IsBlocked = request.IsBlocked,
                        Motivo = request.Motivo,
                        FechaRegistro = DateTime.UtcNow
                    };

                    session.Store(blockedIp);
                }
                else
                {
                    blockedIp.Ip = ip;
                    blockedIp.IsBlocked = request.IsBlocked;
                    blockedIp.Motivo = request.Motivo;
                    blockedIp.FechaRegistro = DateTime.UtcNow;

                    session.Update(blockedIp);
                }

                await session.SaveChangesAsync();

                return Results.Ok(new
                {
                    message = request.IsBlocked
                        ? "IP bloqueada correctamente"
                        : "IP desbloqueada correctamente",
                    blockedIp.Ip,
                    blockedIp.IpNumero,
                    blockedIp.IsBlocked,
                    blockedIp.Motivo,
                    blockedIp.FechaRegistro
                });
            })
            .WithName("BlockIp")
            .WithSummary("Bloquear o desbloquear IP")
            .WithDescription("Cambia el estado de bloqueo de una IP.");
        }
    }
}