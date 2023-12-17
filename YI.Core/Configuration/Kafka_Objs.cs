using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YI.Core.Configuration
{
    public class KafkaModel
    {
        public bool UseProducer { get; set; }
        public ProducerSettings Producer { get; set; }
        public bool UseConsumer { get; set; }
        public ConsumerSettings Consumer { get; set; }
        public bool IsConsumerSubscribe { get; set; }
        public Topics Topics { get; set; }
    }

    public class ProducerSettings
    {
        public string BootstrapServers { get; set; }
        public string SaslMechanism { get; set; }
        public string SecurityProtocol { get; set; }
        public string SaslUsername { get; set; }
        public string SaslPassword { get; set; }
    }
    public class ConsumerSettings
    {
        public string BootstrapServers { get; set; }
        public string SaslMechanism { get; set; }
        public string SecurityProtocol { get; set; }
        public string SaslUsername { get; set; }
        public string SaslPassword { get; set; }
        public string GroupId { get; set; }
    }

    public class Topics
    {
        public string TopicName { get; set; }
    }
}
