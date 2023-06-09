using Confluent.Kafka;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Helpers.Kafka
{
    public class KafkaConsumer
    {
        private readonly string _topicName;
        private readonly ConsumerConfig _consumerConfig;
        private readonly string _connectionString;

        public KafkaConsumer(string topicName, ConsumerConfig consumerConfig, string connectionString)
        {
            _topicName = topicName;
            _consumerConfig = consumerConfig;
            _connectionString = connectionString;
        }

        public async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
            {
                consumer.Subscribe(_topicName);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(cancellationToken);
                    var sqlQuery = result.Message.Value;

                    using (var connection = new SqlConnection(_connectionString))
                    using (var command = new SqlCommand(sqlQuery, connection))
                    {
                        await connection.OpenAsync(cancellationToken);
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }
        }
    }
}
