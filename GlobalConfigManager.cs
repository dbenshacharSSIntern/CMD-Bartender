using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PasswordBasedAuthLogon
{
    internal static class GlobalConfigManager
    {
        private static string ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.bartenderconfig.json";
        private static bool configFileExists = false;

        private static string CurrentProfile;
        private static string Username;
        private static string Password;
        private static string ApplicationID;
        private static string SecretID;
        private static string Directory;
        private static string Website;

        private static JSONProfile jsonProfile;

        public static void Initialize()
        {
            if (!File.Exists(ConfigPath))
            {
                CreateConfigFile();
            }

            try
            {
                JObject jsonObj = (JObject) JToken.ReadFrom(new JsonTextReader(File.OpenText(ConfigPath)));
                jsonProfile = jsonObj.ToObject<JSONProfile>();

                CurrentProfile = jsonProfile.CurrentProfile;

                Username = "";
                Password = "";
                ApplicationID = "";
                SecretID = "";
                Directory = "";
                Website = "";
                configFileExists = true;
            }
            catch
            {
                Console.WriteLine("Warning: Config file not found for user and may not contain correct information.");
            }
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

        public static string GetDirectoryEntry()
        {
            return "librarian://Main/" + GetDirectory();
        }

        private static void SetDirectory(string value)
        {
            Directory = value;
        }

        private static void ChangeGlobalInfo(string NewValue, int LineNumber)
        {
            try
            {
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

        public static void CreateConfigFile()
        {
            var jProfile = new JObject(new JProperty("CurrentProfile", "n/a"),
                new JProperty("Aliuses",
                new JArray(new JObject(
                    new JProperty("Email", "n/a"),
                    new JProperty("Password", "n/a"),
                    new JProperty("ApplicationID", "n/a"),
                    new JProperty("SecretID", "n/a"),
                    new JProperty("Directory", "n/a"),
                    new JProperty("Website", "n/a")
            ))));

            saveProfile(jProfile);
        }

        public static void saveProfile(JObject json)
        {
            using (StreamWriter file = File.CreateText(ConfigPath))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                json.WriteTo(writer);
            }
        }

        public static bool GetConfigFileExists()
        {
            return configFileExists;
        }
    }

    public class Alius
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ApplicationID { get; set; }
        public string SecretID { get; set; }
        public string Directory { get; set; }
        public string Website { get; set; }

    }

    public class JSONProfile
    {
        public string CurrentProfile { get; set; }
        public List<Alius> Aliuses { get; set; }
    }
}
