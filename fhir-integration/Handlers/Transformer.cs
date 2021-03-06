using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace fhir_integration
{
    class Transformer
    {
        private SqlConnection connection { get; set; }
        private Connector connector { get; set; }
        private ConfigurationHandler config { get; set; }
        public int errorCount { get; set; }
        public int outageDuration { get; set; }

        public Transformer(Connector connector, ConfigurationHandler config)
        {
            this.connector = connector;
            this.config = config;
            errorCount = 0;
            outageDuration = 0;
        }

        // Building connection string for DB
        public void InitDb()
        {

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = config.db;
            builder.UserID = config.dbUserId;
            builder.Password = config.dbPassword;
            builder.InitialCatalog = config.dbCatalog;

            SqlConnection conn = new SqlConnection(builder.ConnectionString);

            connection = conn;

        }

        // Handles unsynced BloodPressureMeasurements in DB
        public void HandleUnsyncedMeasurements()
        {
            List<BloodPressureMeasurements> unsyncedMeasurements;

            using (Model1 context = new Model1(connection.ConnectionString))
            {
                unsyncedMeasurements = context.BloodPressureMeasurements
                    .Where(m => m.fhirSynced == 0 && m.isDeleted == 0)
                    .ToList();

            }

            Console.WriteLine("Unsynced measurements count: " + unsyncedMeasurements.Count);
            string measurementIds = "";
            int successCount = 0;

            foreach (BloodPressureMeasurements record in unsyncedMeasurements)
            {
                int userId = record.patientId;
                Dictionary<string, string> patient = ParsePatient(userId);

                int savedMeasurementId = connector.SaveFhirObservation(patient, record);
                bool success = TagAsSynced(savedMeasurementId);
                if (success)
                {
                    measurementIds += savedMeasurementId.ToString() + " ";
                    successCount++;
                }

            }

            Console.WriteLine("Synced measurements count: " + successCount.ToString());
            Console.WriteLine("Synced measurements - Application DB IDs: " + measurementIds);

            config.AddLog("Synced measurements: " + successCount.ToString() + " - Application DB IDs: " + measurementIds);
        }

        // Handle Patients in DB without FHIR id
        public void HandlePatientsWithoutFhir()
        {
            List<Users> noFhirPatients;

            using (Model1 context = new Model1(connection.ConnectionString))
            {
                noFhirPatients = context.Users
                    .Where(u => u.fhirId == null && u.isDeleted == 0 && u.isDoctor == 0)
                    .ToList();
            }

            if (noFhirPatients.Any()) // If there are Patients without FHIR ID
            {
                foreach (var p in noFhirPatients)
                {

                    using (Model1 context = new Model1(connection.ConnectionString))
                    {
                        // Manual join
                        var patientRecord = context.Patients
                            .Where(pat => pat.patientId == p.userId)
                            .FirstOrDefault();

                        if (patientRecord == null) continue; // not a patient, admin nor doctor 

                        var userRecord = p;

                        var city = context.Cities
                        .Where(c => c.cityId == patientRecord.cityId)
                        .FirstOrDefault();

                        var doctorRecord = context.DoctorView
                            .Where(doc => doc.doctorId == patientRecord.assignedDoctor)
                            .FirstOrDefault();

                        string fhirId = connector.GetPatientFhirId(patientRecord, userRecord, city, doctorRecord); // retrieves existing FHIR entity or creates new one
                        UpdateFhirId(fhirId, p.userId);

                        config.AddLog("Patient: " + patientRecord.nationalIdentificationNumber + " - new FHIR ID: " + fhirId);
                        Console.WriteLine("Patient: " + patientRecord.nationalIdentificationNumber + " - new FHIR ID: " + fhirId);

                    }

                }
            }


        }

        // Handle Doctors in DB without FHIR id
        public void HandleDoctorsWithoutFhir()
        {
            List<Users> noFhirDoctors;

            using (Model1 context = new Model1(connection.ConnectionString))
            {
                noFhirDoctors = context.Users
                    .Where(u => u.fhirId == null && u.isDeleted == 0 && u.isDoctor == 1)
                    .ToList();
            }

            if (noFhirDoctors.Any()) // If there are doctors without FHIR ID
            {
                foreach (var d in noFhirDoctors)
                {
                    using (Model1 context = new Model1(connection.ConnectionString))
                    {

                        // Manual join
                        var doctorRecord = context.Doctors
                            .Where(doc => doc.doctorId == d.userId)
                            .FirstOrDefault();

                        var userRecord = d;

                        var city = context.Cities
                            .Where(c => c.cityId == doctorRecord.workingPlaceCity)
                            .FirstOrDefault();

                        string fhirId = connector.GetDoctorFhirId(doctorRecord, userRecord, city); // retrieves existing FHIR entity or creates new one

                        UpdateFhirId(fhirId, d.userId);

                        config.AddLog("Doctor: " + doctorRecord.evidenceNumber + " - new FHIR ID: " + fhirId);
                        Console.WriteLine("Doctor: " + doctorRecord.evidenceNumber + " - new FHIR ID: " + fhirId);

                    }

                }
            }
        }

        // Finds all info about patient upon ID from DB, if needed gets FHIR ID from FHIR server
        public Dictionary<string, string> ParsePatient(int userId)
        {

            using (Model1 context = new Model1(connection.ConnectionString))
            {
                var patientRecord = context.Patients
                    .Where(p => p.patientId == userId)
                    .First();

                var userRecord = context.Users
                   .Where(u => u.userId == userId)
                   .First();

                // Can be refactored - Dictionary is based of old logic without Entity Framework model
                var patient = new Dictionary<string, string>();

                string nationalIdentificationNumber = patientRecord.nationalIdentificationNumber;
                nationalIdentificationNumber = Regex.Replace(nationalIdentificationNumber, @"[^\d]", "");
                patient.Add("nationalIdentificationNumber", nationalIdentificationNumber);
                patient.Add("assignedDoctorId", patientRecord.assignedDoctor.ToString());
                patient.Add("nameTitle", userRecord.nameTitle);
                patient.Add("fhirId", userRecord.fhirId);
                patient.Add("firstName", userRecord.firstName);
                patient.Add("lastName", userRecord.lastName);

                return patient; // firstName, lastName, nameTitle, fhirId, assignedDoctorId, nationalIdentificationNumber

            }

        }

        // Tags as synced in app DB
        public bool TagAsSynced(int measurementId)
        {

            using (Model1 context = new Model1(connection.ConnectionString))
            {
                var measurement = context.BloodPressureMeasurements
                    .Where(m => m.measurementId == measurementId)
                    .First();

                measurement.fhirSynced = 1;

                context.SaveChanges();
                Console.WriteLine("Measurement synced - Application DB ID: " + measurementId.ToString());
                return true;
            }


        }

        // Updates matched FHIR ID in app DB
        public void UpdateFhirId(string fhirId, int userId)
        {
            using (Model1 context = new Model1(connection.ConnectionString))
            {

                var userWithoutFhirId = context.Users
                    .Where(u => u.userId == userId)
                    .First();

                userWithoutFhirId.fhirId = fhirId;
                context.SaveChanges();

            }

        }

        // Synchronize data
        public void Sync()
        {

            Console.WriteLine("\n* Sync start - " + DateTime.Now.ToString());
            config.AddLog("Sync start");

            using (Model1 context = new Model1(connection.ConnectionString))
            {
                var patients = context.Patients; // Load the Patients table
                
                if (patients.Any()) // If the Patients table is not empty, do sync (usually initial start)
                {
                    HandleDoctorsWithoutFhir();
                    HandlePatientsWithoutFhir();
                    HandleUnsyncedMeasurements();
                }
                else
                {
                    Console.WriteLine("No patients in the application database. Skipping the synchronization.");
                    config.AddLog("No patients in the database");
                }

            }

            Console.WriteLine("* Sync end - " + DateTime.Now.ToString() +"\n");
            config.AddLog("Sync end");

        }

        // Test purposes - reset all FHIR info in DB
        public void ResetDbSynced()
        {

            try
            {
                using (Model1 context = new Model1(connection.ConnectionString))
                {
                    var measurements = context.BloodPressureMeasurements.ToList();
                    var fhir = context.Users.ToList();

                    foreach (var m in measurements)
                    {
                        m.fhirSynced = 0;
                    }

                    foreach (var f in fhir)
                    {
                        f.fhirId = null;
                    }

                    context.SaveChanges();

                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }



        }
    }




}
