using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;


namespace fhir_integration
{
    class Connector
    {
        private FhirClient client { get; set; }

        public Connector()
        {

        }

        public void initFhirConnection()
        {
            client = new FhirClient("http://abuelo.ictm.albertov.cz/Spark/fhir");
            client.PreferredFormat = ResourceFormat.Json;
        }

        public void SaveFhirObservation()
        {

        }

        public void getFhirId(string subject, string identifier)
        {
            Console.WriteLine(identifier);

        }
    }
}
