﻿using PasswordBasedAuthLogon;
using System;
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

            if (Command == null || isHelp)
            {
                return "Here is a list of available commands:\n" +
                    "dir\n" +
                    "config\n" +
                    "cd\n" +
                    "del\n" +
                    "return\n" +
                    "status";
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

            return result.Message;
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
            var result = BartenderManager.TestDir(pathModification);

            if (result.Status)
            {
                GlobalConfigManager.ChangeGlobalDirectory(pathModification);
            }
            return result.Message;
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