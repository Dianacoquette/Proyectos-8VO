using System.Text.Json;
using Carter;
using Marten;
using MediatR;
using Sensores.Api.Utils;
using Sensores.Api.Models.Datos;

namespace Sensores.Api.Models.Datos.CreateData
{
    public record CreateDataRequest(
        string ClientId,
        double Temperatura,
        double Humedad,
        DateTime FechaEnvio,
        string? IpDispositivo
    );

    public record CreateDataResponse(
        Guid Id,
        string Message
    );

    public class CreateDataEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/datas", async (
                CreateDataRequest request,
                ISender sender,
                HttpContext httpContext) =>
            {
                var session = httpContext.RequestServices.GetRequiredService<IDocumentSession>();

                var clientIp = httpContext.Connection.RemoteIpAddress?
                    .MapToIPv4()
                    .ToString();

                var containerIp = httpContext.Connection.LocalIpAddress?
                    .MapToIPv4()
                    .ToString();

                var jsonOriginal = JsonSerializer.Serialize(request);

                Console.WriteLine("========== NUEVA PETICIÓN /datas ==========");
                Console.WriteLine($"JSON recibido: {jsonOriginal}");
                Console.WriteLine($"ClientId recibido: {request.ClientId}");
                Console.WriteLine($"IP enviada por ESP32: {request.IpDispositivo}");
                Console.WriteLine($"IP origen detectada por API: {clientIp}");
                Console.WriteLine($"IP contenedor/API: {containerIp}");

                // ================================
                // VALIDAR IP BLOQUEADA
                // ================================
                var ipDispositivoNumero = IpHelper.ConvertIpToNumber(request.IpDispositivo);
                var clientIpNumero = IpHelper.ConvertIpToNumber(clientIp);

                var ipBloqueada = await session.Query<BlockedIp>()
                    .FirstOrDefaultAsync(x =>
                        x.IsBlocked &&
                        (
                            x.IpNumero == ipDispositivoNumero ||
                            x.IpNumero == clientIpNumero
                        )
                    );

                if (ipBloqueada != null)
                {
                    Console.WriteLine($"🚫 IP bloqueada: {ipBloqueada.Ip} → enviando 403");
                    return Results.StatusCode(StatusCodes.Status403Forbidden);
                }

                // ================================
                // REGISTRAR O VALIDAR DISPOSITIVO
                // ================================
                var clientId = request.ClientId.Trim().ToLower();

                var device = await session.Query<Device>()
                    .FirstOrDefaultAsync(d => d.ClientId.ToLower() == clientId);

                if (device == null)
                {
                    Console.WriteLine($"✅ Device nuevo registrado automáticamente: {request.ClientId}");

                    device = new Device
                    {
                        Id = Guid.NewGuid(),
                        ClientId = request.ClientId,
                        Ip = request.IpDispositivo,
                        IsBlocked = false,
                        LastSeen = DateTime.UtcNow
                    };

                    session.Store(device);
                    await session.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine($"📡 Device encontrado: {device.ClientId} - Bloqueado: {device.IsBlocked}");

                    if (device.IsBlocked)
                    {
                        Console.WriteLine($"🚫 Device bloqueado: {device.ClientId} → enviando 403");
                        return Results.StatusCode(StatusCodes.Status403Forbidden);
                    }

                    device.Ip = request.IpDispositivo;
                    device.LastSeen = DateTime.UtcNow;

                    await session.SaveChangesAsync();
                }

                // ================================
                // GUARDAR LECTURA DEL SENSOR
                // ================================
                var command = new CreateDataCommand(
                    request.ClientId,
                    request.Temperatura,
                    request.Humedad,
                    request.FechaEnvio,

                    request.IpDispositivo,
                    IpHelper.ConvertIpToNumber(request.IpDispositivo),

                    clientIp,
                    IpHelper.ConvertIpToNumber(clientIp),

                    containerIp,
                    jsonOriginal
                );

                var result = await sender.Send(command);

                return Results.Created(
                    $"/datas/{result.Id}",
                    new CreateDataResponse(
                        result.Id,
                        "Registro guardado correctamente"
                    )
                );
            })
            .WithName("InsertarData")
            .Produces<CreateDataResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Insertar dato de sensor")
            .WithDescription("Guarda temperatura, humedad, ClientId, IP dispositivo, IP origen, IP contenedor y JSON original recibido.");
        }
    }
}