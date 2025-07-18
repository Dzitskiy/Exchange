using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Contracts.Models
{
    public class OrderDto
    {

        public string Operation { get; set; }

        public string InstrumentId { get; set; }

        public string TradeMode { get; set; }

        public string ClientOrderId { get; set; }

        public string Side { get; set; }

        public string OrderType { get; set; }

        public string Price { get; set; }

        public string Size { get; set; }
    }
}
