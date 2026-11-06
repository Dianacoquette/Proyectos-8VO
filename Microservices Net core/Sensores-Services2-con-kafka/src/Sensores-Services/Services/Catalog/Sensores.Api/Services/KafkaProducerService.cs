using Confluent.Kafka;

namespace Sensores.Api.Services
{
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public KafkaProducerService(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "kafka:9092";
            _topic = configuration["Kafka:Topic"] ?? "lecturas-sensores";

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task EnviarDatoAsync(string mensaje)
        {
            Console.WriteLine("🔥 ENTRE AL PRODUCER");

            Console.WriteLine("================================");
            Console.WriteLine($"ENVIANDO A KAFKA (topic: {_topic})");
            Console.WriteLine(mensaje);

            var result = await _producer.ProduceAsync(
                _topic,
                new Message<string, string>
                {
                    Value = mensaje
                });

            Console.WriteLine($"Topic: {result.Topic}");
            Console.WriteLine($"Offset: {result.Offset}");
            Console.WriteLine("MENSAJE ENVIADO");
            Console.WriteLine("================================");
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(5));
            _producer?.Dispose();
        }
    }
}
