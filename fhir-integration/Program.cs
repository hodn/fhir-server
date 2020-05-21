using fhir_integration.Handlers;
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
            Console.WriteLine("FHIR Integrator");

            ConfigurationHandler config = new ConfigurationHandler();

            try
            {
                config.LoadConfig(); // Loads settings
                config.CreateLogFile(); // Create log file in chosen directory

            }
            catch (Exception e)
            {
                Console.WriteLine("Application not configured: " + e.Message);
                config.AddLog(e.Message);
            }
            

            Connector connector = new Connector(config);
            Transformer transformer = new Transformer(connector, config);
            EmailHandler emailHandler = new EmailHandler(config.email);

            try
            {
                connector.InitFhirConnection(); // defined route to FHIR server
                transformer.InitDb(); // defined connection string to DB
            }
            catch(Exception e)
            {
                Console.WriteLine("Database or FHIR server not connected: " + e.Message);
                config.AddLog(e.Message);
            }
           
                
            int intervalToMillis = config.interval * 60 * 1000;
            int retryIntervalToMillis = config.retryInterval * 60 * 1000;

            Timer syncInterval = new Timer(intervalToMillis); // normal interval
            Timer retryInterval = new Timer(retryIntervalToMillis); // retry interval
            syncInterval.Elapsed += new ElapsedEventHandler(syncIntervalElapsed);
            retryInterval.Elapsed += new ElapsedEventHandler(syncIntervalElapsed);

            void syncIntervalElapsed(object sender, ElapsedEventArgs e)
            {
                Run(); // triggered by timer
            }

            Console.WriteLine("Press any key to run the initial FHIR sync");
            Console.ReadKey();

            Run(); // Inital sync 

            void Run()
            {
                // If ran out of retries (reached normal interval) - send email and reset the interval
                if (transformer.errorCount >= (config.interval / config.retryInterval))
                {
                    transformer.errorCount = 0; // reset error count
                    syncInterval.Start(); // trigger normal interval
                    retryInterval.Stop(); // stop retry interval
                    emailHandler.Send("FHIR sync not available", "Dear Sir or Madam, \n \n the FHIR synchronization was not available during the last " + config.interval + " minutes. Please, check the service. \n Thank you for understanding. \n \n Your, FHIR Integrator");
                    config.AddLog("Service not available for " + config.interval.ToString() + " mins." + " Sent notification email to: " + config.email);
                    Console.WriteLine("Sent notification email to: " + config.email);
                }
                else
                {
                    try
                    {
                        transformer.Sync();
                        syncInterval.Start(); // trigger normal interval
                        retryInterval.Stop(); // stop retry interval
                        transformer.errorCount = 0; // reset error count
                    }
                    catch (Exception e)
                    {
                        config.AddLog(e.Message); // Log to file
                        Console.WriteLine(e.Message);
                        syncInterval.Stop(); // stop normal interval
                        retryInterval.Start(); // trigger retry interval
                        transformer.errorCount++;
                    }
                }

               
            }
                 


            Console.ReadKey();
        }

    }
}
