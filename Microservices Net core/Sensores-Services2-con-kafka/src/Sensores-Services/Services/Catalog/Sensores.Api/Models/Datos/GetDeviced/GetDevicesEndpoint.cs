using Carter;
using Marten;
using Sensores.Api.Models.Datos;

namespace Sensores.Api.Models.Datos.GetDeviced
{
    public class GetDevicesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/devices", async (IDocumentSession session) =>
            {
                var devices = await session.Query<Device>()
                    .OrderByDescending(d => d.LastSeen)
                    .ToListAsync();

                return Results.Ok(devices);
            })
            .WithName("GetDevices")
            .WithSummary("Lista todos los dispositivos")
            .WithDescription("Obtiene los dispositivos registrados con su estado de bloqueo.");

            app.MapGet("/devices/block-ip", async (IDocumentSession session) =>
            {
                var ips = await session.Query<BlockedIp>()
                    .OrderByDescending(x => x.FechaRegistro)
                    .ToListAsync();

                return Results.Ok(ips);
            })
            .WithName("GetBlockedIps")
            .WithSummary("Lista las IPs registradas")
            .WithDescription("Obtiene las IPs bloqueadas o desbloqueadas registradas.");
        }
    }
}