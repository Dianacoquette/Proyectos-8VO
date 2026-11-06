using Confluent.Kafka;
using Marten;
using Sensores.Api.Models;
using System.Text.Json;

namespace Sensores.Api.Consumers
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IDocumentStore _documentStore;
        private readonly string _topic;

        public KafkaConsumerService(IDocumentStore documentStore, IConfiguration configuration)
        {
            _documentStore = documentStore;

            var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "host.docker.internal:9092";
            var groupId = configuration["Kafka:GroupId"] ?? "sensores-api-group";
            _topic = configuration["Kafka:Topic"] ?? "lecturas-sensores";

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"Kafka Consumer iniciado (topic: {_topic})");

            _consumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var resultado = _consumer.Consume(stoppingToken);

                    Console.WriteLine("MENSAJE RECIBIDO DE KAFKA");
                    Console.WriteLine(resultado.Message.Value);

                    var data = JsonSerializer.Deserialize<SensorData>(resultado.Message.Value);

                    if (data != null)
                    {
                        using var session = _documentStore.LightweightSession();

                        session.Store(data);

                        await session.SaveChangesAsync(stoppingToken);

                        Console.WriteLine("Guardado en PostgreSQL desde el consumer");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR DEL CONSUMER DE KAFKA:");
                    Console.WriteLine(ex.Message);

                    await Task.Delay(1000, stoppingToken);
                }
            }

            _consumer.Close();
        }

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}
