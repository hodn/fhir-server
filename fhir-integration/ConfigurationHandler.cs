using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace fhir_integration
{
    class ConfigurationHandler
    {
        public string configPath { get; set; }
        public DateTime startTime { get; set; }
        public int interval { get; set; }
        public int retryInterval { get; set; }
        public string email { get; set; }
        public string logDirectory { get; set; }


        public ConfigurationHandler(string configPath)
        {
            this.configPath = configPath;
        }

        public void LoadConfig()
        {
            try
            {
                XmlDocument configDoc = new XmlDocument();
                configDoc.Load(configPath);

                XmlNodeList xnList = configDoc.GetElementsByTagName("config");

                foreach (XmlNode node in xnList)
                {
                    startTime = DateTime.ParseExact(node["startTime"].InnerText, "H:mm", null, System.Globalization.DateTimeStyles.None);
                    interval = int.Parse(node["interval"].InnerText);
                    retryInterval = int.Parse(node["retryInterval"].InnerText);
                    email = node["email"].InnerText;
                    logDirectory = node["logDirectory"].InnerText;

                    Console.WriteLine("\n----Configuration loaded----");
                    Console.WriteLine("Start at: " + startTime.ToString());
                    Console.WriteLine("Interval: " + interval.ToString() + " mins");
                    Console.WriteLine("Recovery interval: " + retryInterval.ToString() + " mins");
                    Console.WriteLine("Notification email: " + email.ToString());
                    Console.WriteLine("Log directory: " + logDirectory.ToString());


                };
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid configuration: " + e.Message);

            }

        }
    }
}
