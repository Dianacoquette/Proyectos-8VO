using Confluent.Kafka;

namespace Sensores.Api.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducerService()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "host.docker.internal:9092"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task EnviarLecturaAsync(string mensaje)
    {
        Console.WriteLine("================================");
        Console.WriteLine("ENVIANDO A KAFKA");
        Console.WriteLine(mensaje);

        var result = await _producer.ProduceAsync(
            "lecturas",
            new Message<string, string>
            {
                Value = mensaje
            });

        Console.WriteLine($"Topic: {result.Topic}");
        Console.WriteLine($"Offset: {result.Offset}");
        Console.WriteLine("MENSAJE ENVIADO");
        Console.WriteLine("================================");
    }



}
