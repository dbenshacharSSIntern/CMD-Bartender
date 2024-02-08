using BasicAuthLogon.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BasicAuthLogon
{
    internal class Program
    {
        static void Main(string[] args)
        {
            args = ParseArgs(args);
            foreach (string arg in args)
            {
                Console.WriteLine(arg);
            }
        }

        static string[] ParseArgs(string[] args)
        {
            return args;
        }
    }
}