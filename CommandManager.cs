using PasswordBasedAuthLogon;
using System;
using System.Linq;

namespace BasicAuthLogon
{
    internal static class CommandManager
        {
        public static String Run(String Command, bool isHelp, String[] args)
        {
            if (Command == "dir" && isHelp)
            {
                return DirCommand.Help();
            }
            if (Command == "dir")
            {
                return DirCommand.Run(args);
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
                    "config\n";
            }

            throw new ArgumentException("Command does not exist.");
        }
    }
    static class DirCommand
    {
        public static String Help()
        {
            return "Run this command to see any files or folders in the current subdirectory";
        }

        public static String Run(String[] args)
        {
            return "";
        }
    }

    static class ConfigCommand
    {
        public static String Help()
        {
            return "Here is how you can rune this command:\n" +
                "config username {NewUserName}\n" +
                "config password {NewPassword}\n" +
                "config appID {NewApplicationID}\n" +
                "config secretID {NewSecretID}\n";
        }

        public static String Run(String[] args)
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
            throw new ArgumentException(args[0] + " is not an aspect of command config.");
        }
    }
}