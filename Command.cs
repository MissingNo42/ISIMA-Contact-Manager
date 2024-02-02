using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact_Manager
{
    /// <summary>
    /// Represents a command
    /// </summary>
    internal class Command
    {
        /// <summary>
        /// The command's name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The command's syntax (for help)
        /// </summary>
        public readonly string Syntax;

        /// <summary>
        /// The command's description (for help)
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// The command's execution function, takes the arguments as string[] and returns true on error / if the arguments are invalid, false otherwise
        /// </summary>
        private Func<string[], bool> Execute { get; }

        /// <summary>
        /// Instantiate a new command
        /// </summary>
        /// <param name="name">The command's name</param>
        /// <param name="syntax">The command's syntax (for help)</param>
        /// <param name="description">The command's description (for help)</param>
        /// <param name="execute">The command's execution function, takes the arguments as string[] and returns true on error / if the arguments are invalid, false otherwise</param>
        public Command(string name, string syntax, string description, Func<string[], bool> execute)
        {
            Name = name;
            Syntax = syntax;
            Description = description;
            Execute = execute;
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="args">The command line argument split by space</param>
        public void Exec(string[] args)
        {
            if (Execute(args)) Console.WriteLine($"Invalid arguments, type \"help {Name}\" to see how to use {Name}");
        }

        /// <summary>
        /// Dispatch the command arguments to the right command if found, otherwise print an error message
        /// </summary>
        /// <param name="args">The command line argument split by space</param>
        /// <param name="commands">Available commands</param>
        static public void Exec(string[] args, Command[] commands)
        {
            Command command = commands.FirstOrDefault(c => c.Name == args[0]);

            if (command == null) Console.WriteLine("Invalid command, type \"help\" to see the list of commands");
            else command.Exec(args);
        }
    }
}
