using BasicAuthLogon.Models;
using Newtonsoft.Json;
using PasswordBasedAuthLogon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BasicAuthLogon
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(GlobalUserManager.GetUsername());
            Console.WriteLine(GlobalUserManager.GetPassword());
            Console.WriteLine(GlobalUserManager.GetApplicationID());
            Console.WriteLine(GlobalUserManager.GetSecretID());
        }
    }
}