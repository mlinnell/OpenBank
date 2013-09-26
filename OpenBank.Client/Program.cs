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
        public static void Main(string[] args)
        {
            while (true)
            {
                // Use accounts.sensitive.template as an example
                // In this git repo, any file ending in ".sensitive" is ignored.
                var jObj = JsonConvert.DeserializeObject(File.ReadAllText(@"C:\temp\accounts.sensitive")) as JObject;
                var hash = jObj.ToObject<Dictionary<string, string>>();
                hash["date_end"] = DateTime.Now.ToString("yyyyMMdd");

                string previousJson = File.ReadAllText(Statement.OutFile);
                var previous = Statement.Parse(previousJson);
                var current = Statement.Load(hash);

                if (current.Transactions.Count > previous.Transactions.Count)
                {
                    var updatedHash = current.Transactions.ToDictionary(item => item.ID, item => item);
                    var existingHash = previous.Transactions.ToDictionary(item => item.ID, item => item);

                    var newKeys = updatedHash.Keys.Where(key => existingHash.ContainsKey(key) == false);
                    foreach (var key in newKeys)
                    {
                        var transaction = updatedHash[key];
                        File.WriteAllText(@"C:\temp\" + transaction.ID, transaction.ToString());
                    }
                }

                Thread.Sleep(TimeSpan.FromHours(1));
            }
        }
    }
}