using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;


namespace fhir_integration
{
    class Connector
    {
        private FhirClient client { get; set; }
        private ConfigurationHandler config { get; set; }

        public Connector(ConfigurationHandler config)
        {
            this.config = config;
        }

        // Load and get connection string to FHIR server from config file
        public void InitFhirConnection()
        {
            client = new FhirClient(config.fhirServer);
        }

        // Upload measurement to FHIR server
        public int SaveFhirObservation(Dictionary<string, string> patient, BloodPressureMeasurements measurement)
        {
            int measurementId = measurement.measurementId;
            var obs = new Observation();

            var id = new Identifier();
            id.System = config.fhirServer;
            id.Value = measurementId.ToString();
            obs.Identifier.Add(id);

            obs.Status = Observation.ObservationStatus.Final;

            var cat = new CodeableConcept("http://terminology.hl7.org/CodeSystem/observation-category", "vital-signs");
            obs.Category = cat;

            var type = new CodeableConcept("", "LOINC 85354-9");
            obs.Code = type;

            DateTime convertedDate = DateTime.Parse(measurement.measurementTs.ToString());
            obs.Effective = new FhirDateTime(convertedDate);

            var sub = new ResourceReference();
            sub.Reference = (config.fhirServer + "/Patient/") + patient["fhirId"];
            sub.Display = patient["nationalIdentificationNumber"] + " " + patient["lastName"] + " " + patient["firstName"];
            obs.Subject = sub;

            var sys = new Observation.ComponentComponent();
            sys.Code = new CodeableConcept("", "LOINC 8480-6", "Systolic blood pressure");
            sys.Value = new FhirString(measurement.sysPressure.ToString());
            obs.Component.Add(sys);

            var dia = new Observation.ComponentComponent();
            dia.Code = new CodeableConcept("", "LOINC 8462-4", "Diastolic blood pressure");
            dia.Value = new FhirString(measurement.diaPressure.ToString());
            obs.Component.Add(dia);

            var hr = new Observation.ComponentComponent();
            hr.Code = new CodeableConcept("", "LOINC 8867-4", "Heart rate");
            hr.Value = new FhirString(measurement.heartRate.ToString());
            obs.Component.Add(hr);

            var createdObs = client.Create(obs);

            return measurementId;

        }

        // Return patient FHIR ID
        public string GetPatientFhirId(Patients patientRecord, Users userRecord, Cities city, DoctorView doctorRecord)
        {
            string rawIdentifier = patientRecord.nationalIdentificationNumber;
            string identifier = Regex.Replace(rawIdentifier, @"[^\d]", ""); // removing excess chars from patient's business identifier (rodne cislo)
            Bundle results = client.Search<Patient>(new string[] { "identifier:exact=" + identifier }); // searching in FHIR DB via patient's business identifier (rodne cislo)
            string fhirId = null;

            if (results.Entry.Count == 0) // No matching FHIR entity
            {
                // Create a new FHIR identity
                fhirId = UploadFhirPatient(patientRecord, userRecord, city, doctorRecord);

                Console.WriteLine("Patient: " + patientRecord.nationalIdentificationNumber + " - FHIR entity created: " + fhirId);

                return fhirId;
            }

            foreach (var e in results.Entry) // FHIR Id exists, return it
            {
                var patient = (Patient)e.Resource;
                fhirId = patient.Id.ToString();
            }
           
            return fhirId;
        }

        // Return doctor FHIR ID
        public string GetDoctorFhirId(Doctors doctorRecord, Users userRecord, Cities city)
        {
            string rawIdentifier = doctorRecord.evidenceNumber.ToString();
            string identifier = Regex.Replace(rawIdentifier, @"[^\d]", ""); // escaping to numbers and letters only
            Bundle results = client.Search<Practitioner>(new string[] { "identifier:exact=" + identifier });
            string fhirId = null;

            if (results.Entry.Count == 0) // No matching FHIR entity
            {
                // Create a new FHIR identity

                fhirId = UploadFhirPractitioner(doctorRecord, userRecord, city);

                Console.WriteLine("Doctor: " + doctorRecord.evidenceNumber + " - FHIR entity created: " + fhirId);

                return fhirId;
            }

            foreach (var e in results.Entry) // FHIR Id exists, return it
            {
                var patient = (Practitioner)e.Resource;
                fhirId = patient.Id.ToString();
            }

            return fhirId;
        }

        // Upload patient to FHIR server
        public string UploadFhirPatient(Patients patientRecord, Users userRecord, Cities city, DoctorView doctorRecord)
        {
            var pat = new Patient();
            pat.Active = true;

            var id = new Identifier();
            id.System = "http://ciselniky.dasta.mzcr.cz/CD_DS4/hypertext/DSBET.htm";
            string rawIdentifier = patientRecord.nationalIdentificationNumber;
            id.Value = Regex.Replace(rawIdentifier, @"[^\d]", ""); // remove excess chars from patient's business identifier (rodne cislo)
            pat.Identifier.Add(id);

            var name = new HumanName().WithGiven(userRecord.firstName).AndFamily(userRecord.lastName);
            name.Use = HumanName.NameUse.Official;
            pat.Name.Add(name);

            var mail = new ContactPoint(ContactPoint.ContactPointSystem.Email, ContactPoint.ContactPointUse.Mobile, userRecord.email);
            mail.Rank = 1;
            var phone = new ContactPoint(ContactPoint.ContactPointSystem.Phone, ContactPoint.ContactPointUse.Mobile, patientRecord.telNumber.ToString());
            phone.Rank = 2;
            pat.Telecom.Add(mail);
            pat.Telecom.Add(phone);

            pat.Gender = patientRecord.sex == 1 ? AdministrativeGender.Male : AdministrativeGender.Female;

            pat.BirthDate = patientRecord.birthDate.ToString("s", System.Globalization.CultureInfo.InvariantCulture);

            var address = new Address()
            {
                Line = new string[] { patientRecord.address },
                City = city.cityName,
                PostalCode = city.zipCode,
                Country = city.countryCode
            };
            pat.Address.Add(address);

            var createdPat = client.Create(pat);
            var fhirId = createdPat.Id;

            AssignPractitioner(fhirId, doctorRecord); // Finds FHIR identity of patient's practitioner and references it

            return fhirId;
        }

        // Upload doctor to FHIR server
        public string UploadFhirPractitioner(Doctors doctorRecord, Users userRecord, Cities city)
        {
            var doc = new Practitioner();
            doc.Active = true;

            var id = new Identifier();
            id.System = "https://www.lkcr.cz/seznam-lekaru-426.html";
            string rawIdentifier = doctorRecord.evidenceNumber.ToString();
            id.Value = Regex.Replace(rawIdentifier, @"[^\d]", ""); ;
            doc.Identifier.Add(id);

            var name = new HumanName().WithGiven(userRecord.firstName).AndFamily(userRecord.lastName);
            name.Prefix = new string[] { doctorRecord.nameTitle };
            name.Use = HumanName.NameUse.Official;
            doc.Name = name;

            doc.Gender = AdministrativeGender.Unknown;

            var mail = new ContactPoint(ContactPoint.ContactPointSystem.Email, ContactPoint.ContactPointUse.Mobile, userRecord.email);
            doc.Telecom.Add(mail);

            var address = new Address()
            {
                Line = new string[] { doctorRecord.workingPlaceAddress },
                City = city.cityName,
                PostalCode = city.zipCode,
                Country = city.countryCode
            };
            doc.Address.Add(address);

            var createdDoc = client.Create(doc);

            return createdDoc.Id;
        }

        // Reference doctor in newly uploaded patient - FHIR 
        public void AssignPractitioner(string patientFhirId, DoctorView doctorRecord)
        {

            var pat = client.Read<Patient>("Patient/" + patientFhirId);
            var reference = new ResourceReference();
            reference.Reference = config.fhirServer + "/Practitioner/" + doctorRecord.fhirId;
            reference.Display = doctorRecord.evidenceNumber + " " + doctorRecord.firstName + " " + doctorRecord.lastName + " " + doctorRecord.email;
            pat.CareProvider.Add(reference);
            var updatedPat = client.Update(pat);

        }

        // Testing purposes
        public void UploadFhirPatientManual()
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

        public void UploadFhirPractitionerManual()
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

        public void AssignPractitionerManual()
        {

            var pat_A = client.Read<Patient>("Patient/3");
            var reference = new ResourceReference();
            reference.Reference = "http://abuelo.ictm.albertov.cz/Spark/fhir/Practitioner/1";
            reference.Display = "Miroslav Milý 1110905143";
            pat_A.CareProvider.Add(reference);
            var updated_pat = client.Update(pat_A);

        }

        // Delete all records on FHIR
        public void DeleteObs()
        {

            var conditions = new SearchParams();
            conditions.Add("status", "final");
            //client.Delete("Observation", conditions);

            var personConditions = new SearchParams().Where("identifier!=0");
            client.Delete("Practitioner", personConditions);
            client.Delete("Patient", personConditions);


        }
    }

}
