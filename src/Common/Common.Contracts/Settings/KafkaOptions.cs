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
    }
}