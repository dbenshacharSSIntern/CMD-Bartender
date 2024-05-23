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
        private static string UserEmail;
        private static string Password;
        private static string ApplicationID;
        private static string SecretID;
        private static string Directory;
        private static string Website;
        private static string OrgizationNameDNS;

        private static JSONProfile jsonProfile;
        private static Alius Profile;
        private static int TargetIndex;

        public static void Initialize()
        {
            if (!File.Exists(ConfigPath))
            {
                CreateConfigFile();
            }

            try
            {

                var reader = File.OpenText(ConfigPath);
                JObject jsonObj = (JObject) JToken.ReadFrom(new JsonTextReader(reader));
                jsonProfile = jsonObj.ToObject<JSONProfile>();

                CurrentProfile = jsonProfile.CurrentProfile;
                TargetIndex = jsonProfile.Aliuses.FindIndex(alius => alius.Name == CurrentProfile);
                if (TargetIndex == -1) 
                {
                    Console.WriteLine("User not found in settings. Switching to first user.");
                    TargetIndex = 0;
                }
                Profile = jsonProfile.Aliuses[TargetIndex];
                 
                UserEmail = Profile.Email;
                Password = Profile.Password;
                ApplicationID = Profile.ApplicationID;
                SecretID = Profile.SecretID;
                Directory = Profile.Directory;
                OrgizationNameDNS = Profile.OrganizationDNSName;
                Website = Profile.Website;
                configFileExists = true;

                reader.Dispose();
            }
            catch
            {
                Console.WriteLine("Warning: Config file not found for user and may not contain correct information.");
            }
        }
        public static string GetUsername()
        {
            return UserEmail;
        }

        private static void SetUsername(string value)
        {
            Profile.Email = value;
        }

        public static string GetPassword()
        {
            return Password;
        }

        private static void SetPassword(string value)
        {
            Profile. Password = value;
        }

        public static string GetApplicationID()
        {
            return ApplicationID;
        }

        private static void SetApplicationID(string value)
        {
            Profile.ApplicationID = value;
        }

        public static string GetSecretID()
        {
            return SecretID;
        }

        private static void SetSecredID(string value)
        {
            Profile.SecretID = value;
        }

        private static void SetWebsite(string value)
        {
            Profile.Website = value;
        }

        public static string GetWebsite()
        {
            return Website;
        }

        public static string GetOrganizationDNS()
        {
            return OrgizationNameDNS;
        }

        private static void SetOrganizationDNSName(string value)
        {
            Profile.OrganizationDNSName = value;
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
            Profile. Directory = value;
        }

        private static void SaveJSON()
        {
            jsonProfile.Aliuses[TargetIndex] = Profile;
            string jOut = JsonConvert.SerializeObject(jsonProfile);
            GC.Collect();
            File.WriteAllText(ConfigPath, jOut);
        }

        public static void ChangeGlobalUsername(string NewValue)
        {
            SetUsername(NewValue);
            SaveJSON();
        }

        public static void ChangeGlobalPassword(string NewValue)
        {
            SetPassword(NewValue);
            SaveJSON();
        }

        public static void ChangeGlobalApplicationID(string NewValue)
        {
            SetApplicationID(NewValue);
            SaveJSON();
        }

        public static void ChangeGlobalSecretID(string NewValue)
        {
            SetSecredID(NewValue);
            SaveJSON();
        }

        public static void ChangeGlobalDirectory(string NewValue)
        {
            SetDirectory(NewValue);
            SaveJSON();
        }

        public static void ChangeGlobalWebsite(string NewValue)
        {
            SetWebsite(NewValue);
            SaveJSON();
        }

        public static void ChangeGlobalOrginationDNSName(string NewValue)
        {
            SetOrganizationDNSName(NewValue);
            SaveJSON();
        }

        public static JObject CreateAlius(string name)
        {
            return new JObject(
                    new JProperty("Name", name),
                    new JProperty("Email", "n/a"),
                    new JProperty("Password", "n/a"),
                    new JProperty("ApplicationID", "n/a"),
                    new JProperty("SecretID", "n/a"),
                    new JProperty("Directory", ""),
                    new JProperty("Website", "n/a"),
                    new JProperty("OrganizationDNSName", "n/a"));
        }

        public static void CreateConfigFile()
        {
            var jProfile = new JObject(new JProperty("CurrentProfile", "n/a"),
                new JProperty("Aliuses",
                new JArray(CreateAlius("n/a"))));

            saveProfile(jProfile);
        }

        public static void SwitchUser(string Name)
        {
            jsonProfile.CurrentProfile = Name;
            SaveJSON();  
        }

        public static string ChangeAlius(string Name)
        {
            if (!jsonProfile.Aliuses.Any(alius => alius.Name == Name))
            {
                jsonProfile.Aliuses.Add(CreateAlius(Name).ToObject<Alius>());
                SwitchUser(Name);
                SaveJSON();
                return "Alius created successfully. No information for alius has been added yet. Account switched to new account.";
            }
            SwitchUser(Name);
            SaveJSON();
            return "Switched to profile.";
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

        public static Alius GetAlius()
        {
            return Profile;
        }
    }

    public class Alius
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ApplicationID { get; set; } 
        public string SecretID { get; set; }
        public string Directory { get; set; }
        public string Website { get; set; }
        public string OrganizationDNSName { get; set; }

    }

    public class JSONProfile
    {
        public string CurrentProfile { get; set; }
        public List<Alius> Aliuses { get; set; }
    }
}
