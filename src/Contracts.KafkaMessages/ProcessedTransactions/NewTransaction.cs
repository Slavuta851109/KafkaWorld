using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.KafkaMessages.ProcessedTransactions
{
    public class NewTransaction
    {
        public string BrandName { get; set; }
        public Guid CustomerId { get; set; }
        public int RandomNumber { get; set; }
    }
}
