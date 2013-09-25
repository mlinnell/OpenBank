using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBank.Client.Entities
{
    public class Transaction
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public TransactionType Type { get; set; }
        public string Date { get; set; }
        public decimal Amount { get; set; }

        public override string ToString()
        {
            return String.Format("{0}|{1}", Name, Amount);
        }
    }
}