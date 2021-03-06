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
            Console.WriteLine("PAMOS FHIR Integrator");

            ConfigurationHandler config = new ConfigurationHandler();

            try
            {
                config.LoadConfig(); // Loads settings
                config.CreateLogFile(); // Create log file in user data FHIR_logs directory

            }
            catch (Exception e)
            {
                Console.WriteLine("Please, check the configuration: " + e.Message);
                Console.WriteLine("Press ANY key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }


            Connector connector = new Connector(config); // FHIR Connector
            Transformer transformer = new Transformer(connector, config); // DB handler and data transformer
            EmailHandler emailHandler = new EmailHandler(config.email);

            connector.InitFhirConnection(); // defined route to FHIR server
            transformer.InitDb(); // defined connection string to DB

            // Reset functions for test purposes
            //transformer.ResetDbSynced();
            //connector.DeleteObs();

            int intervalToMillis = config.interval * 60 * 1000; // mins to milis
            int retryIntervalToMillis = config.retryInterval * 60 * 1000; // mins to milis

            Timer syncInterval = new Timer(intervalToMillis); // normal interval
            Timer retryInterval = new Timer(retryIntervalToMillis); // retry interval
            syncInterval.Elapsed += new ElapsedEventHandler(syncIntervalElapsed);
            retryInterval.Elapsed += new ElapsedEventHandler(syncIntervalElapsed);

            void syncIntervalElapsed(object sender, ElapsedEventArgs e)
            {
                Run(); // triggered by timer
            }

            Console.WriteLine("=== Press ANY key to run the initial FHIR sync ===");
            Console.ReadKey();
            Console.WriteLine("\n=== If you wish to exit and end the synchronization, press ENTER ===\n");
            Console.WriteLine("-----------------------------------------");

            Run(); // Inital sync 
           
            void Run()
            {
                // If ran out of retries (reached normal interval) - send email and reset the interval
                if (transformer.errorCount >= (config.interval / config.retryInterval))
                {
                    transformer.errorCount = 0; // reset error count
                    syncInterval.Start(); // trigger normal interval
                    retryInterval.Stop(); // stop retry interval
                    transformer.outageDuration += config.interval; // add to the outage time (minutes)
                    emailHandler.Send("FHIR sync not available", "Dear Sir or Madam, \n \n the FHIR synchronization was not available during the last " + transformer.outageDuration.ToString() + " minutes. Please, check the service. \n \n Thank you for understanding. \n \n Kind regards,\n FHIR Integrator");
                    Console.WriteLine(transformer.outageDuration.ToString());
                    config.AddLog("Service not available for " + transformer.outageDuration.ToString() + " mins." + " Sent notification email to: " + config.email);
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
                        transformer.outageDuration = 0; // accumulated outage time reset
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

                Console.WriteLine("-----------------------------------------");
            }

            Console.ReadLine();
        }

    }
}
