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

        // Loading configuration from XML file
        public void LoadConfig()
        {

            XmlDocument configDoc = new XmlDocument();
            configDoc.Load(@"config.xml");

            XmlNodeList xnList = configDoc.GetElementsByTagName("config");

            foreach (XmlNode node in xnList)
            {
                interval = int.Parse(node["interval"].InnerText);
                retryInterval = int.Parse(node["retryInterval"].InnerText);
                email = node["email"].InnerText;
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
                Console.WriteLine("\n");
                Console.WriteLine("Database: " + db);
                Console.WriteLine("Database user ID: " + dbUserId);
                Console.WriteLine("Database catalog: " + dbCatalog);
                Console.WriteLine("Database password: " + hiddenPassword);
                Console.WriteLine("FHIR server: " + fhirServer);
                Console.WriteLine("\n");

            };



        }

        // Creating log file for the current instance in user data dir
        public void CreateLogFile()
        {
            try
            {
                DateTime time = DateTime.Now;
                string[] timestamp = time.ToString("s").Split('T'); 
                string fileName = timestamp[0] + ".txt"; // makes file name from date
                string logDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\FHIR_logs\"; // gets user data directory
                Directory.CreateDirectory(logDirectory); // creates FHIR logs folder if it does not exist

                logPath = Path.Combine(logDirectory + fileName); // combine directory and filename to write

                if (!File.Exists(logPath))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(logPath))
                    {
                        sw.WriteLine("{0}; {1} ", DateTime.Now.ToString(), "Initial start"); // First log in file, first start of the app of the day
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
                    // Appends logs to an existing log file.
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
