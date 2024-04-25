using PasswordBasedAuthLogon;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;

namespace BasicAuthLogon
{
    internal static class CommandManager
        {
        public static string Run(String Command, bool isHelp, String[] args)
        {
            if (Command == "return" && isHelp)
            {
                return ReturnCommand.Help();
            }
            if (Command == "return")
            {
                return ReturnCommand.Run(args);
            }

            if (Command == "status" && isHelp)
            {
                return StatusCommand.Help();
            }
            if (Command == "status")
            {
                return StatusCommand.Run(args);
            }

            if (Command == "dir" && isHelp)
            {
                return DirCommand.Help();
            }
            if (Command == "dir")
            {
                return DirCommand.Run(args);
            }

            if (Command == "del" && isHelp)
            {
                return DelCommand.Help();
            }
            if (Command == "del")
            {
                return DelCommand.Run(args);
            }

            if (Command == "dload" && isHelp)
            {
                return DownloadCommand.Help();
            }
            if (Command == "dload")
            {
                return DownloadCommand.Run(args);
            }

            if (Command == "uload" && isHelp)
            {
                return UploadCommand.Help();
            }
            if (Command == "uload")
            {
                return UploadCommand.Run(args);
            }

            if (Command == "cd" && isHelp)
            {
                return CDCommand.Help();
            }
            if (Command == "cd")
            {
                return CDCommand.Run(args);
            }

            if (Command == "config" && isHelp)
            {
                return ConfigCommand.Help();
            }
            if (Command == "config")
            {
                return ConfigCommand.Run(args);
            }

            if (Command == "mkdir" && isHelp)
            {
                return MakeDirCommand.Help();
            }
            if (Command == "mkdir")
            {
                return MakeDirCommand.Run(args);
            }

            if (Command == "switch" && isHelp)
            {
                return SwitchCommand.Help();
            }
            if (Command == "switch")
            {
                return SwitchCommand.Run(args);
            }

            if (Command == null || isHelp)
            {
                return "Here is a list of available commands:\n" +
                    "dir\n" +
                    "config\n" +
                    "cd\n" +
                    "del\n" +
                    "dload\n" +
                    "uload\n" +
                    "mkdir\n" +
                    "return\n" +
                    "status" +
                    "switch";
            }

            throw new ArgumentException("Command does not exist.");
            }
    }
    static class DirCommand
    {
        public static string Help()
        {
            return "Run this command to see any files or folders in the current subdirectory.";
        }

        public static string Run(string[] args)
        {
            if (args.Length > 0)
            {
                throw new ArgumentException("No arguments are needed for dir command.");
            }
            BartenderManager.Initalize();
           
            return BartenderManager.DisplayFolderDir(GlobalConfigManager.GetDirectoryEntry()).Result;
        }
    }

    static class DelCommand
    {
        public static string Help()
        {
            return "Run this command to delete a file from the directory.";
        }

        public static string Run(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Only the target file name or path is needed for deletion.");
            }
            BartenderManager.Initalize();
            string targetFile = args[0];
            if (!targetFile.StartsWith(GlobalConfigManager.GetDirectoryEntry()))
            {
                targetFile = GlobalConfigManager.GetDirectoryEntry() + targetFile;
            }

            return BartenderManager.CloudDelete(targetFile).Result;
        }
    }

    static class DownloadCommand
    {
        public static string Help()
        {
            return "Run this command to download a file.";
        }

        public static string Run(string[] args)
        {
            string downloadPath;
            if (args.Length > 2)
            {
                throw new ArgumentException("Only the target file name and path to download on local machine are needed.");
            }
            string targetFile = args[0];
            if (args.Length == 1)
            {
                downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            } else
            {
                downloadPath = args[1];
            }
            BartenderManager.Initalize();   

            if (!targetFile.StartsWith(GlobalConfigManager.GetDirectoryEntry()))
            {
                targetFile = GlobalConfigManager.GetDirectoryEntry() + targetFile;
            }

            return BartenderManager.CloudDownload(targetFile, downloadPath).Result;
        }
    }

    static class UploadCommand
    {
        public static string Help()
        {
            return "Run this command to upload a file.";
        }

        public static string Run(string[] args)
        {
            if (args.Length > 1)
            {
                throw new ArgumentException("Only the target file path on your local machine is needed.");
            }
            string targetFilePath = args[0];
            if (!targetFilePath.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)))
            {
                targetFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\" + targetFilePath;
            }

            BartenderManager.Initalize();
            var result = BartenderManager.CloudUpload(targetFilePath);
            result.Wait();
            if (result.IsCompleted)
            {
                return "Success.";
            }
            return "Failure.";
        }
    }

    static class ReturnCommand
    {
        public static string Help()
        {
            return "Run this to restart from the beginning path of bartender.";
        }

        public static string Run(string[] args)
        {
            if (args.Length > 0)
            {
                throw new ArgumentException("No arguments are needed for return command.");
            }
            GlobalConfigManager.ChangeGlobalDirectory("");

            BartenderManager.Initalize();
            var result = BartenderManager.TestDir(GlobalConfigManager.GetDirectoryEntry());
             result.Wait();
            return result.Result.Message;
        }
    }

    static class StatusCommand
    {
        public static string Help()
        {
            return "Run this to find current path in bartender.";
        }

        public static string Run(string[] args)
        {
            if (args.Length > 0)
            {
                throw new ArgumentException("No arguments are needed for status command.");
            }
            return GlobalConfigManager.GetDirectoryEntry();
        }
    }

    static class SwitchCommand
    {
        public static string Help()
        {
            return "Run this to switch to a profile or create one if it doesn't exist already.";
        }

        public static string Run(string[] args)
        {
            if (args.Length > 1)
            {
                throw new ArgumentException("Enter the email of the profile you wish to switch to.");
            }
            return GlobalConfigManager.GetDirectoryEntry();
        }
    }

    static class CDCommand
    {
        public static string Help()
        {
            return "Run this to enter a subdirectory.";
        }

        public static string Run(string[] args)
        {
            if (args.Length > 1)
            {
                throw new ArgumentException("Only one argument is needed for cd command.");
            }
            var pathModification = args[0];
            if (pathModification == "..")
            {
                var lastIndex = GlobalConfigManager.GetDirectory().LastIndexOf("/");
                lastIndex = Math.Max(0, lastIndex);
                pathModification = GlobalConfigManager.GetDirectory().Substring(0, lastIndex);
                GlobalConfigManager.ChangeGlobalDirectory(pathModification);
            }
            else
            {
                pathModification = $"{GlobalConfigManager.GetDirectory()}{pathModification}/";
            }
            BartenderManager.Initalize();
            var result = BartenderManager.TestDir("librarian://Main/" + pathModification);
            result.Wait();

            if (result.Result.Status)
            {
                GlobalConfigManager.ChangeGlobalDirectory(pathModification);
            }
            return result.Result.Message;
        }
    }

    static class MakeDirCommand
    {
        public static string Help()
        {
            return "Run this to create a directory.";
        }

        public static string Run(string[] args)
        {
            if (args.Length > 1)
            {
                throw new ArgumentException("Only one argument is needed for makir command.");
            }
            var folderName = args[0];
            BartenderManager.Initalize();
            var result = BartenderManager.MakeFolder(folderName);
            result.Wait();
            return result.Result.Message;
        }
    }

    static class ConfigCommand
    {
        public static string Help()
        {
            return "Here is how you can rune this command:\n" +
                "config username {NewUserName}\n" +
                "config password {NewPassword}\n" +
                "config appID {NewApplicationID}\n" +
                "config secretID {NewSecretID}\n" +
                "config website {NewWebsite}\n";
        }

        public static string Run(string[] args)
        {
            if (!GlobalConfigManager.GetConfigFileExists())
            {
                GlobalConfigManager.CreateConfigFile();
            }
            if (args.Length > 2)
            {
                throw new ArgumentException("Config only requires 2 commands.");
            }
            else if (args[0] == "username")
            {
                GlobalConfigManager.ChangeGlobalUsername(args[1]);
                return "username changed succesfully";
            }
            else if (args[0] == "password")
            {
                GlobalConfigManager.ChangeGlobalPassword(args[1]);
                return "password changed succesfully";
            }
            else if (args[0] == "appID")
            {
                GlobalConfigManager.ChangeGlobalApplicationID(args[1]);
                return "appID changed succesfully";
            }
            else if (args[0] == "secretID")
            {
                GlobalConfigManager.ChangeGlobalSecretID(args[1]);
                return "secretID changed succesfully";
            }
            else if (args[0] == "website")
            {
                GlobalConfigManager.ChangeGlobalWebsite(args[1]);
                return "website changed succesfully";
            }
            throw new ArgumentException(args[0] + " is not an aspect of command config.");
        }
    }
}