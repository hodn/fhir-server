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
        public int interval { get; set; }
        public int retryInterval { get; set; }
        public string email { get; set; }
        public string logDirectory { get; set; }
        public string logPath { get; set; }
        public string db { get; set; }
        public string dbUserId { get; set; }
        public string dbCatalog { get; set; }
        public string dbPassword { get; set; }
        public string fhirServer { get; set; }


        public ConfigurationHandler(string configPath)
        {
            this.configPath = configPath;
        }


        // Loading configuration from XML file
        public void LoadConfig()
        {
            try
            {
                XmlDocument configDoc = new XmlDocument();
                configDoc.Load(configPath);

                XmlNodeList xnList = configDoc.GetElementsByTagName("config");

                foreach (XmlNode node in xnList)
                {
                    interval = int.Parse(node["interval"].InnerText);
                    retryInterval = int.Parse(node["retryInterval"].InnerText);
                    email = node["email"].InnerText;
                    logDirectory = node["logDirectory"].InnerText;
                    db = node["db"].InnerText;
                    dbUserId = node["dbUserId"].InnerText;
                    dbCatalog = node["dbCatalog"].InnerText;
                    dbPassword = node["dbPassword"].InnerText;
                    fhirServer = node["fhirServer"].InnerText;

                    string hiddenPassword = "";

                    for (int i = 0; i < dbPassword.Length; i++)
                    {
                        hiddenPassword += "*";
                    }


                    Console.WriteLine("\n----Configuration loaded----");
                    Console.WriteLine("Interval: " + interval.ToString() + " mins");
                    Console.WriteLine("Recovery interval: " + retryInterval.ToString() + " mins");
                    Console.WriteLine("Notification email: " + email);
                    Console.WriteLine("Log directory: " + logDirectory);
                    Console.WriteLine("\n");
                    Console.WriteLine("Database: " + db);
                    Console.WriteLine("Database user ID: " + dbUserId);
                    Console.WriteLine("Database catalog: " + dbCatalog);
                    Console.WriteLine("Database password: " + hiddenPassword);
                    Console.WriteLine("FHIR server: " + fhirServer);
                    Console.WriteLine("\n");


                };
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid configuration: " + e.Message);

            }

        }

        // Creating log file for the current instance
        public void CreateLogFile()
        {
            try
            {
                DateTime time = DateTime.Now;
                string fileName = "/FHIR_Logs_" + time.ToString("d") + ".txt";
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

        // Adding logs to existing log file
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
