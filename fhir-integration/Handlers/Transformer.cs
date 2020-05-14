using System;
using System.Collections.Generic;
using System.Data;
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

        public Transformer(Connector connector, ConfigurationHandler config)
        {
            this.connector = connector;
            this.config = config;
            errorCount = 0;
        }

        // Building connection string for DB
        public void InitDb()
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = config.db;
                builder.UserID = config.dbUserId;
                builder.Password = config.dbPassword;
                builder.InitialCatalog = config.dbCatalog;

                SqlConnection conn = new SqlConnection(builder.ConnectionString);

                connection = conn;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Finds unsynced BloodPressureMeasurements in DB
        public List<BloodPressureMeasurements> GetUnsyncedData()
        {
            using (Model1 context = new Model1(connection.ConnectionString))
            {
                var unsyncedData = context.BloodPressureMeasurements
                    .Where(m => m.fhirSynced == 0 && m.isDeleted == 0)
                    .ToList();
                return unsyncedData;
            }
        }

        // Finds all info about patient upon ID from DB, if needed gets FHIR ID from FHIR server
        public Dictionary<string, string> ParsePatient(int userId)
        {

            using (Model1 context = new Model1(connection.ConnectionString))
            {
                // Manual join - Views are broken
                var patientRecord = context.Patients
                    .Where(p => p.patientId == userId)
                    .First();

                var userRecord = context.Users
                   .Where(u => u.userId == userId)
                   .First();

                var patient = new Dictionary<string, string>();

                string nationalIdentificationNumber = patientRecord.nationalIdentificationNumber.ToString();
                nationalIdentificationNumber = Regex.Replace(nationalIdentificationNumber, @"[^\d]", "");
                patient.Add("nationalIdentificationNumber", nationalIdentificationNumber);
                patient.Add("assignedDoctorId", patientRecord.assignedDoctor.ToString());

                if (userRecord.fhirId == null)
                {
                    string fhirId = connector.getFhirId(patient["nationalIdentificationNumber"]);
                    if (fhirId == null) return null;
                    patient.Add("fhirId", fhirId);
                    UpdateFhirId(fhirId, userId);
                }
                else
                {
                    patient.Add("fhirId", userRecord.fhirId);
                }

                patient.Add("firstName", userRecord.firstName);
                patient.Add("lastName", userRecord.lastName); 

                return patient; // firstName, lastName, fhirId, assignedDoctorId, nationalIdentificationNumber

            }

        }

        // Tags as synced in app DB
        public bool TagAsSynced(int measurementId)
        {
            if (measurementId != 0)
            {
                using (Model1 context = new Model1(connection.ConnectionString))
                {
                    var measurement = context.BloodPressureMeasurements
                        .Where(m => m.measurementId == measurementId)
                        .First();

                    measurement.fhirSynced = 1;

                    context.SaveChanges();
                    Console.WriteLine("Measurement synced - ID: " + measurementId.ToString());
                    return true;
                }
            }
            else
            {
                Console.WriteLine("Skipping the measurement");
                return false;
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

            Console.WriteLine(" \n Sync start - " +  DateTime.Now.ToString());
            config.AddLog("Sync start");
            var unsyncedData = GetUnsyncedData();
            Console.WriteLine("Unsynced measurements count: " + unsyncedData.Count);
            string measurementIds = "";
            int successCount = 0;

            foreach (BloodPressureMeasurements record in unsyncedData)
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
            Console.WriteLine("Synced measurements - IDs: " + measurementIds);

            config.AddLog("Synced " + successCount.ToString() + " - " + measurementIds);

            Console.WriteLine("Sync end - " + DateTime.Now.ToString());
            config.AddLog("Sync end" );

            Console.WriteLine("\n");

        }
    }


        

}
