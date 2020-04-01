using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fhir_integration
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("FHIR Integrator \n");

            ConfigurationHandler config = new ConfigurationHandler("C:/Users/Hoang/Desktop/test.xml");
            config.LoadConfig();
            
            Console.ReadKey();
        }
    }
}
