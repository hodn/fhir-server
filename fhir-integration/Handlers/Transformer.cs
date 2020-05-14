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

        public Transformer(Connector connector, ConfigurationHandler config)
        {
            this.connector = connector;
            this.config = config;
        }

        // Building connection string for DB
        public void ConnectDB()
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
                    patient.Add("fhirId", fhirId);
                    updateFhirId(fhirId, userId);
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
        
        public Dictionary<string, string> parsePractitioner(int userId)
        {
            // Finds FHIR ID, first name, last name - USERS table
            // Finds national Identification number - Doctors table
            // If FHIR ID is NULL -> getFHIR ID
            // Returns FHIR ID, first name, last name, evidenceNumber
            return null;
        }

        public void TagAsSynced(int measurementId)
        {

            using (Model1 context = new Model1(connection.ConnectionString))
            {
                var measurement = context.BloodPressureMeasurements
                    .Where(m => m.measurementId == measurementId)
                    .First();

                measurement.fhirSynced = 1;

                context.SaveChanges();
            }
        }

        public void updateFhirId(string fhirId, int userId)
        {
            try
            {
                connection.Open();
                string query = "UPDATE Users SET fhirId=@fhirId WHERE userId=@userId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@fhirId", fhirId);
                    command.ExecuteNonQuery();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                connection.Close();
            }
            
        }
    }
        

}
