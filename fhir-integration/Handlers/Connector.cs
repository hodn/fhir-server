using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
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
        }

        public void SaveFhirObservation()
        {

        }

        public string getFhirId(string identifier)
        {
            try
            {
                Bundle results = client.Search<Patient>(new string[] { "identifier:exact=" + identifier });
                string fhirId = null;

                if(results.Entry.Count() == 0) return fhirId;

                foreach (var e in results.Entry)
                {
                    // Let's write the fully qualified url for the resource to the console:
                    Console.WriteLine("Full url for this resource: " + e.FullUrl);

                    var patient = (Patient)e.Resource;

                    // Do something with this patient, for example write the family name that's in the first
                    // element of the name list to the console:
                    fhirId = patient.Id.ToString();
                }

                return fhirId;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            
        }
    }
}
