using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            config.LoadConfig();
            config.CreateLogFile();

            Connector connector = new Connector();
            Transformer transformer = new Transformer(connector);

            connector.initFhirConnection();
            connector.testUploadPractitioner();
            transformer.ConnectDB("abuelo.ictm.albertov.cz", "test", "test", "test");
            transformer.getUnsyncedData(); // foreach in unsyncedData - parse Patient, Performer, core data - parse Observation, send it to FHIR server, log it in DB if synced
            transformer.parsePatient(11);

            Console.ReadKey();
        }
    }
}
