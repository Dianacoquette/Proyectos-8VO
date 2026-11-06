using System.Text.Json;
using Sensores.Api.Services;

namespace Sensores.Api.Models.Datos.CreateData
{
    public record CreateDataCommand(
        string ClientId,
        double Temperatura,
        double Humedad,
        DateTime FechaEnvio,
        string? IpDispositivo,
        long IpDispositivoNumero,
        string? IpOrigen,
        long IpOrigenNumero,
        string? IpContenedor,
        string? JsonOriginal
    ) : ICommand<CreateDataResult>;

    public record CreateDataResult(Guid Id);

    internal class CreateDataCommandHandler(
        IDocumentSession documentSession,
        IKafkaProducerService kafkaProducerService)
      : ICommandHandler<CreateDataCommand, CreateDataResult>
    {
        public async Task<CreateDataResult> Handle(
            CreateDataCommand request,
            CancellationToken cancellationToken)
        {
            // 🔴 VALIDAR BLOQUEO AQUÍ TAMBIÉN
            var device = await documentSession.Query<Device>()
                .FirstOrDefaultAsync(d => d.ClientId == request.ClientId, cancellationToken);

            if (device != null && device.IsBlocked)
            {
                Console.WriteLine("🚫 BLOQUEADO EN HANDLER - NO SE GUARDA");

                throw new Exception("Dispositivo bloqueado");
                // o también puedes usar:
                // return null;
                // o un Result de error si manejas eso
            }

            var data = new SensorData
            {
                Id = Guid.NewGuid(),
                ClientId = request.ClientId,
                Temperatura = request.Temperatura,
                Humedad = request.Humedad,
                FechaEnvio = request.FechaEnvio,
                FechaRecepcion = DateTime.UtcNow,

                IpDispositivo = request.IpDispositivo,
                IpDispositivoNumero = request.IpDispositivoNumero,

                IpOrigen = request.IpOrigen,
                IpOrigenNumero = request.IpOrigenNumero,

                IpContenedor = request.IpContenedor,

                JsonOriginal = request.JsonOriginal
            };

            // 🟢 En vez de guardar directo en PostgreSQL, publicamos a Kafka.
            // El KafkaConsumerService es quien persiste el registro de forma asíncrona.
            var mensaje = JsonSerializer.Serialize(data);
            await kafkaProducerService.EnviarDatoAsync(mensaje);

            return new CreateDataResult(data.Id);
        }
    }
}