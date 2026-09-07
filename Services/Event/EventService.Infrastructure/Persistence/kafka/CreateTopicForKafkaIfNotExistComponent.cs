using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventApp.Shared.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace EventService.Infrastructure.Persistence.kafka
{
    public static class CreateTopicForKafkaIfNotExistComponent
    {
        public static void Create(this WebApplicationBuilder builder)
        {
            try
            {
                var bootstrapServers = builder.Configuration.GetValue<string>("Kafka:BootstrapServers")
                ?? throw new InvalidOperationException("Kafka 'BootstrapServers' not found.");

                using var adminClient = new AdminClientBuilder(
                    new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();

                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                if (metadata.Topics.Any(t => t.Topic == Constants.BookingConfirmed))
                {
                    Console.WriteLine("The booking-confirmed topic already exists");
                }

                // Создаём топик, если его нет
                adminClient.CreateTopicsAsync(new[]
                {
                new TopicSpecification
                    {
                        Name = Constants.BookingConfirmed,
                        NumPartitions = 3,
                        ReplicationFactor = 1
                    }
                });

                Console.WriteLine(
                    "The booking-confirmed topic was successfully created with 3 batches and a replication factor of 1.");

            }
            catch (CreateTopicsException ex)
            {
                Console.WriteLine(
                    $"Couldn't create a booking-confirmed topic: {ex.Results.FirstOrDefault()?.Error.Reason}"
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Critical error when checking/creating a booking-confirmed topic");
            }
        }
    }
}
