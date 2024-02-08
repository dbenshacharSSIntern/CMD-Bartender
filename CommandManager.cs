using System;

namespace BasicAuthLogon
{
    internal static class CommandManager
    {
        public static String Help(String Command, bool isHelp)
        {
            if (Command == null || isHelp)
            {
                return "Here is a list of available commands:" +
                    "dir";
            }
            if (Command == "dir" && isHelp)
            {
                return DirCommand.Help();
            }
            if (Command == "dir")
            {
                return DirCommand.run();
            }

            return "";
        }
    }
    static class DirCommand
    {
        public static String Help()
        {
            return "Run this command to see any files or folders in the current subdirectory";
        }

        public static String run()
        {
            return "";
        }
    }
}