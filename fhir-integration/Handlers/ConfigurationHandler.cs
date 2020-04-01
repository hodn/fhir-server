using System;
using System.Collections.Generic;
using System.IO;
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
        public string logPath { get; set; }


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
                    Console.WriteLine("\n");


                };
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid configuration: " + e.Message);

            }

        }

        public void CreateLogFile()
        {
            try
            {
                string fileName = "/FHIR_Logs_" + startTime.ToString("d") + ".txt";
                logPath = Path.Combine(logDirectory + fileName);

                if (!File.Exists(logPath))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(logPath))
                    {
                        sw.WriteLine("{0}; {1} ", DateTime.Now.ToString(), "Initial start");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Log file error: " + e.Message);

            }


        }

        public void AddLog(string log)
        {
            try
            {
                if (File.Exists(logPath))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.AppendText(logPath))
                    {
                        sw.WriteLine("{0}; {1}", DateTime.Now.ToString(), log);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Log file error: " + e.Message);

            }


        }
    }

}
