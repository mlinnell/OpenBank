using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenBank.Client.Entities;

namespace OpenBank.Client
{
    public class Program
    {
        private const string OutFile = @"C:\temp\credit.out";

        public static void Main(string[] args)
        {
            while (true)
            {
                // Use accounts.sensitive.template as an example
                // In this git repo, any file ending in ".sensitive" is ignored.
                JObject hash = JsonConvert.DeserializeObject(File.ReadAllText(@"C:\temp\accounts.sensitive")) as JObject;
                hash["date_end"] = DateTime.Now.ToString("yyyyMMdd");
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

                string existingJson = File.ReadAllText(OutFile);
                Statement existing = Statement.Parse(existingJson);
                Statement updated = Statement.Parse(responseString);

                if (updated.Transactions.Count > existing.Transactions.Count)
                {
                    var updatedHash = updated.Transactions.ToDictionary(item => item.ID, item => item);
                    var existingHash = existing.Transactions.ToDictionary(item => item.ID, item => item);

                    var newKeys = updatedHash.Keys.Where(key => existingHash.ContainsKey(key) == false);
                    foreach (var key in newKeys)
                    {
                        var transaction = updatedHash[key];
                        File.WriteAllText(@"C:\temp\" + transaction.ID, transaction.ToString());
                    }
                }

                File.WriteAllText(OutFile, responseString);

                Thread.Sleep(TimeSpan.FromHours(1));
            }
        }
    }
}