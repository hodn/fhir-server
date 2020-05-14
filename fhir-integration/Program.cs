using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Timers;

namespace fhir_integration
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("FHIR Integrator \n");

            Console.WriteLine("Enter configuration file path: ");
            string configPath = "C:/Users/Hoang/Desktop/test.xml";

            ConfigurationHandler config = new ConfigurationHandler(configPath);


            try
            {   

                config.LoadConfig();
                config.CreateLogFile();

                Connector connector = new Connector(config);
                Transformer transformer = new Transformer(connector, config);

                connector.InitFhirConnection();
                transformer.ConnectDB();

                Timer syncInterval = new Timer(1000 * 60 * config.interval);
                syncInterval.Elapsed += new ElapsedEventHandler(syncIntervalElapsed);
                Timer retryInterval = new Timer(1000 * 60 * config.retryInterval);
                retryInterval.Elapsed += new ElapsedEventHandler(syncIntervalElapsed);

                void syncIntervalElapsed(object sender, ElapsedEventArgs e)
                {
                    transformer.Sync();
                }

                Console.WriteLine("Database and FHIR server connection initialized \n");

                Console.WriteLine("Press any key to run the initial FHIR sync");
                Console.ReadKey();

                transformer.Sync();
                syncInterval.Start();


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                config.AddLog(e.Message);
            }

            Console.ReadKey();
        }

    }
}
