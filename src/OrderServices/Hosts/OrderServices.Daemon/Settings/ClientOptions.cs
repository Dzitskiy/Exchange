using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderServices.Daemon.Settings
{
    /// <summary>
    /// Настройки для клиентов.
    /// </summary>
    public class ClientOptions
    {
        /// <summary>
        /// Адрес API.
        /// </summary>
        public string OrderApiUrl { get; set; }

        /// <summary>
        /// Таймаут запроса к API.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    }
}
