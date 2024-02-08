﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PasswordBasedAuthLogon
{
    internal static class GlobalUserManager
    {
        private static String ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.bartenderconfig.txt";
        private static StreamReader ConfigReader = new StreamReader(ConfigPath);

        private static String Username = RetriveUsername();
        private static String Password = RetrivePassword();
        private static String ApplicationID = RetriveApplicationID();
        private static String SecretID = RetriveSecretID();

        private static String BaseRetriveConfigInfo(String RetrieveKey, String RetriveName)
        {
            String Line;
            String[] Lines;
            try
            {
                Line = ConfigReader.ReadLine();
                if (Line == null) { throw new ArgumentException("Config file line is blank."); }
                Lines = Line.Split(" ");
                if (Lines[0] != RetrieveKey)
                {
                    throw new ArgumentException(RetriveName + " not properly defined in config file.");
                }
                return Lines[1];
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException("Config file for user is not found");
            }
        }

        private static String RetriveUsername()
        {
            return BaseRetriveConfigInfo("username,", "Username");
        }

        private static String RetrivePassword()
        {
            return BaseRetriveConfigInfo("password,", "Password");
        }

        private static String RetriveApplicationID()
        {
            return BaseRetriveConfigInfo("applicationID,", "ApplicationID");
        }

        private static String RetriveSecretID()
        {
            return BaseRetriveConfigInfo("secretID,", "SecretID");
        }

        public static String GetUsername()
        {
            return Username;
        }

        private static void SetUsername(String value)
        {
            Username = value;
        }

        public static String GetPassword()
        {
            return Password;
        }

        private static void SetPassword(String value)
        {
            Password = value;
        }

        public static String GetApplicationID()
        {
            return ApplicationID;
        }

        private static void SetApplicationID(String value)
        {
            ApplicationID = value;
        }

        public static String GetSecretID()
        {
            return SecretID;
        }

        private static void SetSecredID(String value)
        {
            SecretID = value;
        }

        public static String GetConfigPath()
        {
            return ConfigPath;
        }
    }
}