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
using OpenBank.Client.Entities;

namespace OpenBank.Client
{
    public class Program
    {
        private static TextInfo TextInfo = new CultureInfo("en-US", false).TextInfo;

        public static void Main(string[] args)
        {
            // Use accounts.sensitive.template as an example
            // In this git repo, any file ending in ".sensitive" is ignored.
            JObject hash = JsonConvert.DeserializeObject(File.ReadAllText(@"C:\temp\accounts.sensitive")) as JObject;
            var postParameters = hash.First.First.ToObject<Dictionary<string, string>>();

            StringBuilder postData = new StringBuilder();
            foreach (var kvp in postParameters)
                if (String.IsNullOrEmpty(postParameters[kvp.Key]) == false)
                    postData.AppendFormat("{0}={1}&", kvp.Key, kvp.Value);

            byte[] data = Encoding.ASCII.GetBytes(postData.ToString().TrimEnd('&'));

            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create("http://localhost:1234/statement");
            httpReq.Method = "POST";
            httpReq.ContentType = "application/x-www-form-urlencoded";
            httpReq.ContentLength = data.Length;
            httpReq.Accept = "application/json";

            using (var stream = httpReq.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            HttpWebResponse response = (HttpWebResponse)httpReq.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            JObject result = JsonConvert.DeserializeObject(responseString) as JObject;

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

            statement.Transactions = new List<Transaction>();
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
        }
    }
}