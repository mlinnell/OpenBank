using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenBank.Client.Entities
{
    public class Statement
    {
        public  const string OutFile = @"C:\temp\credit.out";
        private static TextInfo TextInfo = new CultureInfo("en-US", false).TextInfo;

        public Statement()
        {
            Transactions = new List<Transaction>();
        }

        public Balance LedgerBalance { get; set; }
        public Balance AvailableBalance { get; set; }
        public List<Transaction> Transactions { get; set; }

        public static Statement Load(Dictionary<string, string> parameters)
        {
            var postData = new StringBuilder();
            foreach (var kvp in parameters)
                if (String.IsNullOrEmpty(parameters[kvp.Key]) == false)
                    postData.AppendFormat("{0}={1}&", kvp.Key, kvp.Value);

            byte[] data = Encoding.ASCII.GetBytes(postData.ToString().TrimEnd('&'));

            var httpReq = (HttpWebRequest)WebRequest.Create("http://localhost:1234/statement");
            httpReq.Method = "POST";
            httpReq.ContentType = "application/x-www-form-urlencoded";
            httpReq.ContentLength = data.Length;
            httpReq.Accept = "application/json";

            using (var stream = httpReq.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)httpReq.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            File.WriteAllText(Statement.OutFile, responseString);

            return Statement.Parse(responseString);
        }

        public static Statement Parse(string json)
        {
            if (String.IsNullOrEmpty(json))
                return new Statement();

            var result = JsonConvert.DeserializeObject(json) as JObject;

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