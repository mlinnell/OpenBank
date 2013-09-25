using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenBank.Client.Entities
{
    public class Statement
    {
        private static TextInfo TextInfo = new CultureInfo("en-US", false).TextInfo;

        public Statement()
        {
            Transactions = new List<Transaction>();
        }

        public Balance LedgerBalance { get; set; }
        public Balance AvailableBalance { get; set; }
        public List<Transaction> Transactions { get; set; }

        public static Statement Parse(string json)
        {
            if (String.IsNullOrEmpty(json))
                return new Statement();

            JObject result = JsonConvert.DeserializeObject(json) as JObject;

            var jStatement = result.First.First;
            var statement = new Statement();
            statement.LedgerBalance = new Balance()
            {
                Amount = Convert.ToDecimal(jStatement[Mappings.LedgerBalance][Mappings.Amount].ToString()),
                Date = jStatement[Mappings.LedgerBalance][Mappings.Date].ToString(),
            };

            statement.AvailableBalance = new Balance()
            {
                Amount = Convert.ToDecimal(jStatement[Mappings.AvailableBalance][Mappings.Amount].ToString()),
                Date = jStatement[Mappings.AvailableBalance][Mappings.Date].ToString(),
            };

            foreach (JObject nestedObj in jStatement[Mappings.Transactions] as JArray)
            {
                statement.Transactions.Add(new Transaction()
                {
                    ID = nestedObj[Mappings.ID].ToString(),
                    Name = nestedObj[Mappings.Name].ToString(),
                    Type = (TransactionType)Enum.Parse(typeof(TransactionType), TextInfo.ToTitleCase(nestedObj["type"].ToString().ToLower())),
                    Date = nestedObj[Mappings.Date].ToString(),
                    Amount = Convert.ToDecimal(nestedObj[Mappings.Amount])
                });
            }

            return statement;
        }

        public override string ToString()
        {
            return String.Format("Balance: {0}/{1}", LedgerBalance, AvailableBalance);
        }
    }
}