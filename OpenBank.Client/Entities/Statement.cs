using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBank.Client.Entities
{
    public class Statement
    {
        public Balance LedgerBalance { get; set; }
        public Balance AvailableBalance { get; set; }
        public List<Transaction> Transactions { get; set; }

        public override string ToString()
        {
            return String.Format("Balance: {0}/{1}", LedgerBalance, AvailableBalance);
        }
    }
}