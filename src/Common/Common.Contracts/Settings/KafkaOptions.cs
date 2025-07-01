using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Contracts.Settings
{
    /// <summary>
    /// Настройки для подключения к Kafka.
    /// </summary>
    public class KafkaOptions
    {
        /// <summary>
        /// Адреса брокеров Kafka.
        /// </summary>
        public required string BootstrapServers { get; set; }

        /// <summary>
        /// Топики, используемые в приложении.
        /// </summary>
        public required Dictionary<string, string> Topics { get; set; }

        /// <summary>
        /// Группа потребителей Kafka.
        /// </summary>
        public string? ConsumerGroup { get; set; }

        /// <summary>
        /// Размер пула потребителей Kafka.
        /// </summary>
        public int ConsumerPoolSize { get; set; } = 3;
    }
}

//public string BootstrapServers { get; set; }
//public string RequestTopic { get; set; }
//public string ResponseTopic { get; set; }
//public string ConsumerGroup { get; set; }
//public int RetryIntervalMs { get; set; }
//public int MaxRetryAttempts { get; set; }