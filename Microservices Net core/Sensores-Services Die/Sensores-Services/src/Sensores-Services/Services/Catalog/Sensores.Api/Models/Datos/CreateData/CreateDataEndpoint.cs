using System.Text.Json;
using Sensores.Api.Utils;

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
                var clientIp = httpContext.Connection.RemoteIpAddress?
                    .MapToIPv4()
                    .ToString();

                var containerIp = httpContext.Connection.LocalIpAddress?
                    .MapToIPv4()
                    .ToString();

                var jsonOriginal = JsonSerializer.Serialize(request);

                var command = new CreateDataCommand(
                    request.ClientId,
                    request.Temperatura,
                    request.Humedad,
                    request.FechaEnvio,

                    // IP real enviada por la ESP32
                    request.IpDispositivo,
                    IpHelper.ConvertIpToNumber(request.IpDispositivo),

                    // IP detectada por la API
                    clientIp,
                    IpHelper.ConvertIpToNumber(clientIp),

                    // IP del contenedor/API
                    containerIp,

                    // JSON original
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