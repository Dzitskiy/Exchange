using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessing.AppServices.Order.Services
{
    public interface IOrderProcessingService
    {
        /// <summary>
        /// Обработка заказа асинхронно, получая сообщение из Kafka.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task ProcessOrderAsync(Message<Ignore, string> message);
    }
}
