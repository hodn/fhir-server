﻿using System;
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
            Transformer transformer = new Transformer();
            transformer.connectDB();

            Console.WriteLine("FHIR Integrator \n");

            Console.WriteLine("Enter configuration file path: ");
            string configPath = Console.ReadLine();

            ConfigurationHandler config = new ConfigurationHandler(configPath);
            config.LoadConfig();
            config.CreateLogFile();

            Console.ReadKey();
        }
    }
}
