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

        public Transformer(Connector connector)
        {
            this.connector = connector;   
        }

        // Building connection string for DB
        public void ConnectDB(string dataSource, string userId, string pass, string catalog)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = dataSource;
                builder.UserID = userId;
                builder.Password = pass;
                builder.InitialCatalog = catalog;

                SqlConnection conn = new SqlConnection(builder.ConnectionString);
                
                connection = conn;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Finds unsynced BloodPressureMeasurements in DB
        public DataTable getUnsyncedData()
        {
            DataTable unsyncedData = new DataTable();
            string queryString = "SELECT * FROM BloodPressureMeasurements WHERE isDeleted=0 AND fhirSynced=0";
            SqlCommand command = new SqlCommand(queryString, connection);

            connection.Open();
     
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(unsyncedData);
                da.Dispose();
            }
            finally
            {
                connection.Close();
            }

            return unsyncedData;
        }

        // Finds all info about patient upon ID from DB, gets FHIR ID from FHIR server
        public Dictionary<string, string> parsePatient(int userId)
        {
            // Finds FHIR ID, first name, last name - USERS table
            // Finds national Identification number - Patients table
            // Returns FHIR ID, first name, last name, NiNO
            var patient = new Dictionary<string, string>();
            string queryStringUsers = "SELECT * FROM Users WHERE userId=" + userId.ToString();
            string queryStringPatients = "SELECT * FROM Patients WHERE patientId=" + userId.ToString();
            SqlCommand commandUsers = new SqlCommand(queryStringUsers, connection);
            SqlCommand commandPatients = new SqlCommand(queryStringPatients, connection);

            try
            {
                connection.Open();
                DataTable resultUser = new DataTable();
                DataTable resultPatients = new DataTable();
                SqlDataAdapter daUsers = new SqlDataAdapter(commandUsers);
                SqlDataAdapter daPatients = new SqlDataAdapter(commandPatients);
                daUsers.Fill(resultUser);
                daPatients.Fill(resultPatients);
                daUsers.Dispose();
                daPatients.Dispose();

                foreach (DataRow row in resultPatients.Rows)
                {
                    string nationalIdentificationNumber = row["nationalIdentificationNumber"].ToString();
                    nationalIdentificationNumber = Regex.Replace(nationalIdentificationNumber, @"[^\d]", "");
                    patient.Add("nationalIdentificationNumber", nationalIdentificationNumber);
                    patient.Add("assignedDoctor", row["assignedDoctor"].ToString());
                }

                foreach (DataRow row in resultUser.Rows)
                {
                    if (row["fhirId"] == DBNull.Value) {
                        string fhirId = connector.getFhirId(patient["nationalIdentificationNumber"]);
                        patient.Add("fhirId", fhirId);
                        updateFhirId(fhirId, userId);
                    };

                    patient.Add("firstName", row["firstName"].ToString());
                    patient.Add("lastName", row["lastName"].ToString());

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

            return patient;

        }
        
        public Dictionary<string, string> parsePractitioner(int userId)
        {
            // Finds FHIR ID, first name, last name - USERS table
            // Finds national Identification number - Doctors table
            // If FHIR ID is NULL -> getFHIR ID
            // Returns FHIR ID, first name, last name, evidenceNumber
            return null;
        }

        public void tagAsSynced(int measurementId)
        {

        }

        public void updateFhirId(string fhirId, int userId)
        {
         
            string query = "UPDATE Users SET fhirId=@fhirId WHERE userId=@userId";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@fhirId", fhirId);
                int radku = command.ExecuteNonQuery();
                Console.WriteLine(fhirId);
            }
        }
    }
        

}
