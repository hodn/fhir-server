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
                    var patient = (Patient)e.Resource;
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

        // Testing purposes
        public void testUploadPatient()
        {
            var pat = new Patient();
            pat.Active = true;

            var id = new Identifier();
            id.System = "http://ciselniky.dasta.mzcr.cz/CD_DS4/hypertext/DSBET.htm";
            id.Value = "8705109876";
            pat.Identifier.Add(id);

            var name = new HumanName().WithGiven("Jan").AndFamily("Novák");
            name.Prefix = new string[] { "Ing." };
            name.Use = HumanName.NameUse.Official;
            pat.Name.Add(name);

            var telecom = new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Mobile, "733742893");
            telecom.Rank = 1;
            var mail = new ContactPoint(ContactPoint.ContactPointSystem.Email, ContactPoint.ContactPointUse.Mobile, "jan.novak@posta.cz");
            mail.Rank = 2;
            pat.Telecom.Add(telecom);
            pat.Telecom.Add(mail);
          
            pat.Gender = AdministrativeGender.Male;

            pat.BirthDate = "1987-05-10";

            var address = new Address()
            {
                Line = new string[] { "Kostelní 23/2" },
                City = "Praha",
                PostalCode = "19000",
                Country = "Czech Republic"
            };
            pat.Address.Add(address);

            var created_pat = client.Create(pat);
        }

        public void testUploadPractitioner()
        {
            var pat = new Practitioner();
            pat.Active = true;

            var id = new Identifier();
            id.System = "https://www.lkcr.cz/seznam-lekaru-426.html";
            id.Value = "1110905143";
            pat.Identifier.Add(id);

            var name = new HumanName().WithGiven("Miroslav").AndFamily("Milý");
            name.Prefix = new string[] { "MuDr." };
            name.Use = HumanName.NameUse.Official;
            pat.Name = name;

            pat.BirthDate = "1969-02-12";

            var mail = new ContactPoint(ContactPoint.ContactPointSystem.Email, ContactPoint.ContactPointUse.Mobile, "miroslav.mily@posta.cz");
            pat.Telecom.Add(mail);

            pat.Gender = AdministrativeGender.Male;

            var created_pat = client.Create(pat);
            Console.WriteLine("Doktor");
        }

        public void update()
        {

            var pat_A = client.Read<Patient>("Patient/3");
            var reference = new ResourceReference();
            reference.Reference = "http://abuelo.ictm.albertov.cz/Spark/fhir/Practitioner/1";
            reference.Display = "Miroslav Milý 1110905143";
            pat_A.CareProvider.Add(reference);
            var updated_pat = client.Update(pat_A);

        }
    }
}
