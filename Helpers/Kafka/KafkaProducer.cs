using Confluent.Kafka;
using System.Threading.Tasks;

namespace WebApp.Helpers.Kafka
{
    public class KafkaProducer
    {
        private readonly ProducerConfig _producerConfig;
        private readonly string _topicName;

        public KafkaProducer(ProducerConfig producerConfig, string topicName)
        {
            _producerConfig = producerConfig;
            _topicName = topicName;
        }

        public async Task ProduceAsync(string sqlQuery)
        {
            using (var producer = new ProducerBuilder<Null, string>(_producerConfig).Build())
            {
                var message = new Message<Null, string> { Value = sqlQuery };
                await producer.ProduceAsync(_topicName, message);
            }
        }
    }
}
