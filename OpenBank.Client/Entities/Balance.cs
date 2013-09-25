using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBank.Client.Entities
{
    public class Balance
    {
        public decimal Amount;
        public string Date;

        public override string ToString()
        {
            return String.Format("{0}|{1}", Date, Amount);
        }
    }
}