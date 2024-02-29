using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PasswordBasedAuthLogon
{
    internal static class GlobalConfigManager
    {
        private static string ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.bartenderconfig.txt";
        private static string[] ConfigReader;
        private static bool configFileExists = false;

        private static string Username;
        private static string Password;
        private static string ApplicationID;
        private static string SecretID;
        private static string Directory;
        private static string Website;

        public static void Initialize() 
        {
            try
            {
                ConfigReader = File.ReadAllLines(ConfigPath);
            } catch {
                File.Create(ConfigPath);
            }
            try
            {
                while (ConfigReader.Length < 6) {
                    ConfigReader.Append("\n");
                }

                Username = RetriveUsername();
                Password = RetrivePassword();
                ApplicationID = RetriveApplicationID();
                SecretID = RetriveSecretID();
                Directory = RetriveDirectory();
                Website = RetriveWebsite();
                configFileExists = true;
            } catch
            {
                Console.WriteLine("Warning: Config file not found for user and may not contain correct information.");
            }
        }

        private static string RetriveUsername()
        {
            return GetConfigFileLine(0);
        }

        private static string RetrivePassword()
        {
            return GetConfigFileLine(1);
        }

        private static string RetriveApplicationID()
        {
            return GetConfigFileLine(2);
        }

        private static string RetriveSecretID()
        {
            return GetConfigFileLine(3);
        }

        private static string RetriveDirectory()
        {
            return GetConfigFileLine(4);
        }

        private static string RetriveWebsite()
        {
            return GetConfigFileLine(5);
        }

        public static string GetUsername()
        {
            return Username;
        }

        private static void SetUsername(string value)
        {
            Username = value;
        }

        public static string GetPassword()
        {
            return Password;
        }

        private static void SetPassword(string value)
        {
            Password = value;
        }

        public static string GetApplicationID()
        {
            return ApplicationID;
        }

        private static void SetApplicationID(string value)
        {
            ApplicationID = value;
        }

        public static string GetSecretID()
        {
            return SecretID;
        }

        private static void SetSecredID(string value)
        {
            SecretID = value;
        }

        public static string GetWebsite()
        {
            return Website;
        }

        private static void SetWebsite(string value)
        {
            Website = value;
        }

        public static string GetConfigPath()
        {
            return ConfigPath;
        }

        public static string GetDirectory()
        {
            return Directory;
        }

        private static void SetDirectory(string value)
        {
            Directory = value;
        }

        private static void ChangeGlobalInfo(string NewValue,int LineNumber)
        {
            try
            {
                WriteConfigFileLine(LineNumber, NewValue);
            }
            catch
            {
                throw new Exception("There is an unexpected error in your config file.");
            }
        }

        public static void ChangeGlobalUsername(string NewValue)
        {
            ChangeGlobalInfo(NewValue, 0);
            SetUsername(NewValue);
        }

        public static void ChangeGlobalPassword(string NewValue)
        {
            ChangeGlobalInfo(NewValue, 1);
            SetPassword(NewValue);
        }

        public static void ChangeGlobalApplicationID(string NewValue)
        {
            ChangeGlobalInfo(NewValue, 2);
            SetApplicationID(NewValue);
        }

        public static void ChangeGlobalSecretID(string NewValue)
        {
            ChangeGlobalInfo(NewValue, 3);
            SetSecredID(NewValue);
        }

        public static void ChangeGlobalDirectory(string NewValue)
        {
            ChangeGlobalInfo(NewValue, 4);
            SetDirectory(NewValue);
        }

        public static void ChangeGlobalWebsite(string NewValue)
        {
            ChangeGlobalInfo(NewValue, 5);
            SetWebsite(NewValue);
        }

        private static string GetConfigFileLine(int LineNumber)
        {
            return ConfigReader[LineNumber];
        }

        private static void WriteConfigFileLine(int LineNumber, string NewValue)
        {
            ConfigReader[LineNumber] = NewValue;
            File.WriteAllLines(GetConfigPath(), ConfigReader);
        }

        public static void CreateConfigFile()
        {
            File.WriteAllLines(GetConfigPath(), new string[6]);
            Initialize();
        }

        public static bool GetConfigFileExists()
        {
            return configFileExists;
        }
    }
}
