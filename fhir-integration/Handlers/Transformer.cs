using System;
using System.Collections.Generic;
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

        public void getUnsyncedData()
        {
            string queryString = "SELECT * FROM Users";
            SqlCommand command = new SqlCommand(queryString, connection);

            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    Console.WriteLine("{0} {1}", reader.GetInt32(0), reader.GetString(1));
                }
            }
            finally
            {
                reader.Close();
            }

            connection.Close();
        }
    }
}
