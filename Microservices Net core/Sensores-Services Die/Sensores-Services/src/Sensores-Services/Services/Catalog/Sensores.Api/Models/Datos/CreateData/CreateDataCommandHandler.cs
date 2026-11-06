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

    internal class CreateDataCommandHandler(IKafkaProducerService kafkaProducerService)
        : ICommandHandler<CreateDataCommand, CreateDataResult>
    {
        public async Task<CreateDataResult> Handle(
            CreateDataCommand request,
            CancellationToken cancellationToken)
        {
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

            var mensaje = System.Text.Json.JsonSerializer.Serialize(data);

            await kafkaProducerService.EnviarLecturaAsync(mensaje);

            return new CreateDataResult(data.Id);
        }
    }
}