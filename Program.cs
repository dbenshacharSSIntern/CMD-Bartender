using BasicAuthLogon.Models;
using Newtonsoft.Json;
using PasswordBasedAuthLogon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        try
            {
                GlobalConfigManager.initialize();
                CommandArgs CMD = ParseArgs(args);
                Console.WriteLine(CMD.Run());
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected exception occured: \n" + ex.Message);
            }
        }

        static CommandArgs ParseArgs(string[] args)
        {
            CommandArgs CMD;
            if (args.Length < 1)
            {
                throw new ArgumentException("1 or more arguments must be passed to program");
            }
            if (args.Length == 1) {
                CMD = new CommandArgs(args[0]);
            } else
            {
                CMD = new CommandArgs(args[0], args[1..]);
            }
            return CMD;
        }
    }
    class CommandArgs
    {
        private string Command;
        private string[] Args;
        public CommandArgs(string command, string[] args)
        {
            Command = command;
            Args = args;
        }

        public CommandArgs(string command)
        {
            Command = command;
            Args = new string[] { "help"};
        }

        public string Run()
        {
            if (Command == null)
            {
                return CommandManager.Run(null, true, Args);
            }
            if (Args.Contains("help"))
            {
                return CommandManager.Run(Command, true, Args);
            }
            return CommandManager.Run(Command, false, Args);
        }
    }
}