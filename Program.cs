using DataManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Contact_Manager
{
	/// <summary>
	/// Command line interface for the contact manager
	/// </summary>
	internal class Program
	{
		/// <summary>
		/// The manager instance
		/// </summary>
		private readonly Manager manager = new Manager();

		/// <summary>
		/// The list of commands
		/// </summary>
		private readonly Command[] commands;

        /// <summary>
        /// Instantiate a new command line interface
        /// </summary>
        public Program()
		{
			commands = [
                new Command("help", "help [<command name>]", "Provides help for the specified command, or list them if none. Commands arguments are <positional>, can be [<optional>], keyword=<keyword>", Help),
                new Command("add", "add <type: folder|contact> <name> [<firstname> <company> <relation: friend|colleague|network|other> <mail address>]", "Adds a folder or a contact to the current folder", Add),
                new Command("rm", "rm <type: folder|contact> <name>", "Removes a folder or a contact, by name, from the current folder", Remove),
                new Command("upd", "upd <type: folder|contact> <name> name=<new-name> [firstname=<firstname>] [company=<company>] [relation=<relation: friend|colleague|network|other>] [email=<mail address>]", "Updates a folder or a contact, by name, from the current folder.", Update),
                new Command("list", "list [.]", "display the whole tree, using the current folder as root if \".\" specified", List),
				new Command("load", "load [<password>]", "load the tree from a save if exists, using the specified password (optional)", Load),
				new Command("save", "save [<password>]", "save the tree, cyphed using the specified password (optional)", Save),
                new Command("cd", "cd [<relative path>]", "change the current working directory to the one specified, or to the root if none", Cd),
                new Command("pwd", "pwd", "show the absolute path of the current working directory", Pwd),
                new Command("exit", "exit", "exit the program without saving", Exit),
			];
		}

		/// <summary>
		/// The program entry point
		/// </summary>
		static void Main()
		{
			new Program().Run();
		}

		/// <summary>
		/// The main loop
		/// </summary>
		private void Run()
		{
			while (true)
			{
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write(manager.Current.GetPath() + "> ");

				Console.ForegroundColor = ConsoleColor.DarkGray;

				List<string> inputList = new List<string>();
				bool escaped = false;

				foreach (string s in Console.ReadLine().Split('"')) // allow spaces in arguments by using double quotes: "xx yy"
				{
                    if (s != "")
                    {
						if (escaped) inputList.Add(s); 
						else inputList.AddRange(s.Split(' ').Where(s => s != ""));
					}

					escaped = !escaped;
				}

				Console.ForegroundColor = ConsoleColor.White;

				if (!escaped)
				{
					Console.WriteLine("Invalid input: missing closing \"");
					continue;
				}

				if (inputList.Count == 0) continue;

				try { 
					Command.Exec(inputList.ToArray(), commands);
				} catch (Exception e)
				{
					if (e.Message == "Exit") break;
					throw e;
				}
			}
		}

        /// <summary>
        /// The help command
        /// </summary>
        /// <param name="args">The command line argument split by space</param>
        /// <returns>true on error / if the arguments are invalid, false otherwise</returns>
        private bool Help(string[] args)
		{
			switch (args.Length)
			{
				case 1:
					foreach (Command command in commands) Console.WriteLine(command.Name + ":\t" + command.Description);
					break;
				case 2:
					foreach (Command command in commands)
					{
						if (command.Name == args[1])
						{
							Console.WriteLine(command.Name + ":\t" + command.Description);
							Console.WriteLine(command.Syntax);
							return false;
						}
					}
					Console.WriteLine($"Help: command \"{args[1]}\" does not exist, type \"help\" to see available commands");
					break;
				default:
					return true;
			}
			return false;
        }

        /// <summary>
        /// The add command
        /// </summary>
        /// <param name="args">The command line argument split by space</param>
        /// <returns>true on error / if the arguments are invalid, false otherwise</returns>
        private bool Add(string[] args)
        {
            if (args.Length >= 2)
            {
                switch (args[1])
                {
                    case "folder":
                        if (args.Length == 3)
                        {
                            try
                            {
                                manager.AddFolder(args[2]);
                            }
                            catch (FormatException e)
                            {
                                Console.WriteLine($"Add: {e.Message}");
                            }
                            catch (DuplicateNameException e)
                            {
                                Console.WriteLine($"Add: {e.Message}");
                            }
                        }
                        else return true;
                        break;

                    case "contact":
                        if (args.Length == 7)
                        {
                            if (Enum.TryParse(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(args[5]), out Relation relation))
                                try
                                {
                                    manager.AddContact(args[2], args[3], args[4], relation, args[6]);
                                }
                                catch (FormatException)
                                {
                                    Console.WriteLine($"Add: invalid email address: \"{args[6]}\"");
                                }
                            else Console.WriteLine("Add: invalid relation, type \"help add\" to see available relations");
                        }
                        else return true;
                        break;

					default:
						return true;
				}
            }
            return false;
        }

        /// <summary>
        /// The remove command
        /// </summary>
        /// <param name="args">The command line argument split by space</param>
        /// <returns>true on error / if the arguments are invalid, false otherwise</returns>
        private bool Remove(string[] args)
        {
            if (args.Length == 3)
            {
                switch (args[1])
                {
                    case "folder":
                        if (!manager.RemoveFolder(args[2])) Console.WriteLine($"Remove: folder \"{args[2]}\" does not exist");
                        break;

                    case "contact":
                        if (!manager.RemoveContact(args[2])) Console.WriteLine($"Remove: contact \"{args[2]}\" does not exist");
                        break;

                    default:
						return true;
                }
            } else return true;
            return false;
		}

		/// <summary>
		/// The update command
		/// </summary>
		/// <param name="args">The command line argument split by space</param>
		/// <returns>true on error / if the arguments are invalid, false otherwise</returns>
		private bool Update(string[] args)
		{
			if (args.Length >= 2)
			{
				switch (args[1])
				{
					case "folder":
						if (args.Length == 4)
						{
							try
							{
								if (!args[3].StartsWith("name=")) return true;
								var folder = manager.Current.GetFolder(args[2]);

								if (folder == null) Console.WriteLine($"Update: folder \"{args[2]}\" does not exist");
								else folder.Name = args[3].Substring(5);
							}
							catch (FormatException e)
							{
								Console.WriteLine($"Update: {e.Message}");
							}
							catch (DuplicateNameException e)
							{
								Console.WriteLine($"Update: {e.Message}");
							}
						}
						else return true;
						break;

					case "contact":
						if (args.Length >= 4)
						{
							var contact = manager.Current.GetContact(args[2]);

							for (int i = 3; i < args.Length; i++)
							{
								if (!args[i].Contains('=')) return true;
								string[] arg = args[i].Split('=');

								switch (arg[0])
								{
									case "name":
										try
										{
											contact.Name = arg[1];
										}
										catch (ArgumentException e)
										{
											Console.WriteLine($"Update: {e.Message}");
										}
										break;

									case "firstname":
										try
										{
											contact.FirstName = arg[1];
										}
										catch (ArgumentException e)
										{
											Console.WriteLine($"Update: {e.Message}");
										}
										break;

									case "company":
										try
										{
											contact.Company = arg[1];
										}
										catch (ArgumentException e)
										{
											Console.WriteLine($"Update: {e.Message}");
										}
										break;

									case "relation":
										if (Enum.TryParse(arg[1], out Relation relation)) contact.Relation = relation;
										else Console.WriteLine("Update: invalid relation, type \"help upd\" to see available relations");
										break;

									case "email":
										try
										{
											contact.MailAddress = arg[1];
										}
										catch (FormatException)
										{
											Console.WriteLine($"Update: invalid email address: \"{arg[1]}\"");
										}
										break;

									default:
										return true;
								}
							}
						}
						else return true;
						break;

					default:
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// The list command
		/// </summary>
		/// <param name="args">The command line argument split by space</param>
		/// <returns>true on error / if the arguments are invalid, false otherwise</returns>
		private bool List(string[] args)
		{
			if (args.Length > 2) return true;
			if (args.Length == 2)
			{
				if (args[1] != ".") return true;
				manager.Current.Write();
			}
			else manager.Root.Write();

			return false;
		}

        /// <summary>
        /// The load command
        /// </summary>
        /// <param name="args">The command line argument split by space</param>
        /// <returns>true on error / if the arguments are invalid, false otherwise</returns>
        private bool Load(string[] args)
		{
			if (args.Length > 2) return true;

			string key = args.Length == 2 ? args[1] : null;
			int i;

			try
			{
				for (i = 2; i < 4; i++)
				{
					if (manager.Load(key)) break;

					Console.Write($"Load: loading failed: Invalid password\nTry {i}/3: Password (leave empty to cancel): ");
					key = Console.ReadLine();
					if (key == "")
					{
						Console.WriteLine("Load: loading canceled");
						return false;
					}
				}
				
				if (i == 4)
				{
					Console.WriteLine("Load: too many invalid password: deleting the database...");
					manager.DeleteSave();                   
				}

			} catch (FileNotFoundException)
			{
				Console.WriteLine("Load: no save found");
			}

			return false;
		}

        /// <summary>
        /// The save command
        /// </summary>
        /// <param name="args">The command line argument split by space</param>
        /// <returns>true on error / if the arguments are invalid, false otherwise</returns>
        private bool Save(string[] args)
		{
			if (args.Length > 2) return true;

			manager.Save(args.Length == 2 ? args[1]: null);

			return false;
		}

        /// <summary>
        /// The cd command
        /// </summary>
        /// <param name="args">The command line argument split by space</param>
        /// <returns>true on error / if the arguments are invalid, false otherwise</returns>
        private bool Cd(string[] args)
		{
			if (args.Length > 2) 
				return true;

			if (args.Length == 1) 
				manager.SetCurrentAsRoot();

			else if (!manager.ChangeCurrent(args[1])) 
				Console.WriteLine($"Cd: \"{args[1]}\" does not exist");

			return false;
        }

        /// <summary>
        /// The pwd command
        /// </summary>
        /// <param name="args">The command line argument split by space</param>
        /// <returns>true on error / if the arguments are invalid, false otherwise</returns>
        private bool Pwd(string[] args)
        {
            if (args.Length > 1) return true;
            Console.WriteLine($"Pwd: {manager.Current.GetPath()}");
			return false;
        }

        /// <summary>
        /// The exit command
        /// </summary>
        /// <param name="args">The command line argument split by space</param>
        /// <returns>true on error / if the arguments are invalid, false otherwise</returns>
        /// <exception cref="Exception">Raised for break the main loop with message "Exit"</exception>
        private bool Exit(string[] args)
        {
            if (args.Length > 1) return true;
            throw new Exception("Exit");
        }
    }
}
