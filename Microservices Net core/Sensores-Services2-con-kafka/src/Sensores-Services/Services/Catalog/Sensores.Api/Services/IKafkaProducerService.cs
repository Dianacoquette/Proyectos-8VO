namespace Sensores.Api.Services
{
    public interface IKafkaProducerService
    {
        Task EnviarDatoAsync(string mensaje);
    }
}
