using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.KafkaMessages.WalletTransactions
{
    public class WalletTransaction
    {
        public string TransactionId { get; set; }
        public DateTime Timestamp { get; set; }
        public string BrandName { get; set; }
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
    }
}
