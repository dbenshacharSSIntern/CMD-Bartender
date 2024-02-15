using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PasswordBasedAuthLogon
{
    internal static class GlobalConfigManager
    {
        private static String ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.bartenderconfig.txt";
        private static String[] ConfigReader;
        private static bool configFileExists = false;

        private static String Username;
        private static String Password;
        private static String ApplicationID;
        private static String SecretID;

        public static void initialize()
        {
            try
            {
                ConfigReader = File.ReadAllLines(ConfigPath);

                Username = RetriveUsername();
                Password = RetrivePassword();
                ApplicationID = RetriveApplicationID();
                SecretID = RetriveSecretID();
                configFileExists = true;
            } catch (Exception ex)
            {
                Console.WriteLine("Warning: Config file not found for user and may not contain correct information.");
            }
        }

        private static String RetriveUsername()
        {
            return GetConfigFileLine(0);
        }

        private static String RetrivePassword()
        {
            return GetConfigFileLine(1);
        }

        private static String RetriveApplicationID()
        {
            return GetConfigFileLine(2);
        }

        private static String RetriveSecretID()
        {
            return GetConfigFileLine(3);
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

        private static void ChangeGlobalInfo(String NewValue,int LineNumber)
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

        public static void ChangeGlobalUsername(String NewValue)
        {
            ChangeGlobalInfo(NewValue, 0);
            SetUsername(NewValue);
        }

        public static void ChangeGlobalPassword(String NewValue)
        {
            ChangeGlobalInfo(NewValue, 1);
            SetPassword(NewValue);
        }

        public static void ChangeGlobalApplicationID(String NewValue)
        {
            ChangeGlobalInfo(NewValue, 2);
            SetApplicationID(NewValue);
        }

        public static void ChangeGlobalSecretID(String NewValue)
        {
            ChangeGlobalInfo(NewValue, 3);
            SetSecredID(NewValue);
        }

        private static String GetConfigFileLine(int LineNumber)
        {
            return ConfigReader[LineNumber];
        }

        private static void WriteConfigFileLine(int LineNumber, String NewValue)
        {
            ConfigReader[LineNumber] = NewValue;
            File.WriteAllLines(GetConfigPath(), ConfigReader);
        }

        public static void CreateConfigFile()
        {
            File.WriteAllLines(GetConfigPath(), new String[4]);
            initialize();
        }

        public static bool GetConfigFileExists()
        {
            return configFileExists;
        }
    }
}
