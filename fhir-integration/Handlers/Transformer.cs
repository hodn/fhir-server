using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fhir_integration
{
    class Transformer
    {
        private SqlConnection connection { get; set; }

        public Transformer()
        {
            
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

        private Dictionary<string, string> parsePatient(int userId)
        {
            // Finds FHIR ID, first name, last name - USERS table
            // Finds national Identification number - Patients table
            // Returns FHIR ID, first name, last name, NiNO
            return null;
        }

        private Dictionary<string, string> parsePractitioner(int userId)
        {
            // Finds FHIR ID, first name, last name - USERS table
            // Finds national Identification number - Doctors table
            // If FHIR ID is NULL -> getFHIR ID
            // Returns FHIR ID, first name, last name, evidenceNumber
            return null;
        }

        public void parseObservation()
        {

        }
    }
        

}
