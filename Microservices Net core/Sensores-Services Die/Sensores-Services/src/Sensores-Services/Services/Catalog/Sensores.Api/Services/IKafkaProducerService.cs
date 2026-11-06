namespace Sensores.Api.Services;

public interface IKafkaProducerService
{
    Task EnviarLecturaAsync(string mensaje);
}
