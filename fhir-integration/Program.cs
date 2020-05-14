using System;
using System.Collections.Generic;
using System.Data;
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

            Connector connector = new Connector(config);
            Transformer transformer = new Transformer(connector, config);

            connector.initFhirConnection();
            transformer.ConnectDB();

            var unsyncedData = transformer.getUnsyncedData();

            foreach (BloodPressureMeasurements record in unsyncedData)
            {
                int userId = record.patientId;
                Dictionary<string, string> patient = transformer.parsePatient(userId);
                //int savedMeasurementId = connector.SaveFhirObservation(patient, row);
                //transformer.tagAsSynced(savedMeasurementId);
            }
            

            Console.ReadKey();
        }
    }
}
