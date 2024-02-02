using Serialize;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace DataManager
{
	/// <summary>
	/// Represents and manages a "file system", through a root folder
	/// </summary>
	public class Manager
	{
		/// <summary>
		/// The current working directory (private)
		/// </summary>
		private Folder current;

		/// <summary>
		/// The root folder
		/// </summary>
		public Folder Root { get; protected set; }

		/// <summary>
		/// The current working directory, take care not to remove it from the arborescence using the root itself, use RemoveCurrent instead
		/// </summary>
		/// <exception cref="ArgumentNullException">Raised if the folder is null</exception>
		/// <exception cref="ArgumentException">Raised if the folder doesn't belong to this arborescence</exception>
		public Folder Current
		{
			get => current;
			set
			{
				if (value == null) throw new ArgumentNullException("The current folder cannot be null");
				if (value.GetRoot() != Root) throw new ArgumentException("The current folder cannot have another root");
				current = value;
			}
		}

		/// <summary>
		/// The number of tries left to load the save file before deleting it
		/// </summary>
		public int TryCount { get; private set; } = 3;

		/// <summary>
		/// The serializer instance
		/// </summary>
		private readonly Serializer<DSO.Item> serializer;

		/// <summary>
		/// The save file path
		/// </summary>
		private readonly string file;

		/// <summary>
		/// Create a new manager
		/// </summary>
		/// <param name="serializerType">Select the serialization type</param>
		public Manager(SerializerType serializerType = SerializerType.XML)
		{
			serializer = SerializerFactory.GetSerializer<DSO.Item>(serializerType, WindowsIdentity.GetCurrent().User.Value);

			Root = new Folder("root");
			current = Root;

			// retrieve the username part of the domain[\subdomain]\username and put it in the file name
			file = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{WindowsIdentity.GetCurrent().Name.Split('\\').Last()}.bin";
		}

		/// <summary>
		/// Add a new folder to the current folder and move to it
		/// </summary>
		/// <param name="name">The new folder name</param>
		/// <exception cref="FormatException">Raised if / or \ in the name</exception>
		/// <exception cref="DuplicateNameException">Raised if the name is already used by another folder</exception>"
		public void AddFolder(string name)
		{
			var n = new Folder(name);
			current.AddItem(n);
			current = n;
		}

		/// <summary>
		/// Add a new contact to the current folder
		/// </summary>
		/// <param name="name">The contact's last name</param>
		/// <param name="firstName">The contact's first name</param>
		/// <param name="company">The contact's company</param>
		/// <param name="relation">The contact's relation</param>
		/// <param name="mailAddress">The contact's mail address</param>
		/// <exception cref="FormatException">Raised if email is invalid</exception>
		/// <exception cref="ArgumentException">Raised if name, first name or company are null or empty</exception>
		public void AddContact(string name, string firstName, string company, Relation relation, string mailAddress)
		{
			current.AddItem(new Contact(name, firstName, company, relation, mailAddress));
		}

		/// <summary>
		/// Change the current working directory to the selected one, supports relative paths (aa/../../bb/./cc) and absolute paths (/aa/../cc).
		/// Support both / and \ as path separator
		/// </summary>
		/// <param name="path">The path, supporting both relative and absolute path</param>
		/// <returns>true if changed, false otherwise</returns>
		public bool ChangeCurrent(string path)
		{
			Folder folder = (path.Count() > 0 && "/\\".Contains(path[0])) ? Root.GetFolder(path.Substring(1)) : Current.GetFolder(path);

			if (folder != null) current = folder;

			return folder != null;
		}

		/// <summary>
		/// Set the current working directory to the root
		/// </summary>
		public void SetCurrentAsRoot()
		{
			current = Root;
		}

		/// <summary>
		/// Remove the current working directory and move to its parent, or clear if the current directory is the root
		/// </summary>
		public void RemoveCurrent()
		{
			if (current == Root) current.Clear();
			else
			{
				((Folder)current.Parent).RemoveItem(current);
				current = (Folder)current.Parent;
			}
		}

		/// <summary>
		/// Remove a folder from the current working directory
		/// </summary>
		/// <param name="name">The directory name</param>
		/// <returns>true if removed, false otherwise</returns>
		public bool RemoveFolder(string name)
		{
			return current.RemoveFolder(name);
		}

		/// <summary>
		/// Remove contact(s) from the current working directory
		/// </summary>
		/// <param name="name">The contact(s)'s name</param>
		/// <returns>true if any contact removed, false otherwise</returns>
		public bool RemoveContact(string name)
		{
			return current.RemoveContact(name) > 0;
		}


		/// <summary>
		/// Load the save file if any
		/// </summary>
		/// <param name="key">The password, use Windows SID if null or empty</param>
		/// <returns>
		/// true if the load was successful, false otherwise
		/// </returns>
		/// <exception cref="FileNotFoundException">Thrown when the save file doesn't exist</exception>
		public bool Load(string key = null)
		{
			if (string.IsNullOrWhiteSpace(key)) key = WindowsIdentity.GetCurrent().User.Value;

			serializer.Key = Encoding.UTF8.GetBytes(key);
			var r = serializer.Deserialize(file);
			if (r != null)
			{
				TryCount = 3;
				Root = current = (Folder)r.ToObject();
			}
			else if (--TryCount <= 0) DeleteSave();

			return r != null;
		}

		/// <summary>
		/// Save the current state of the manager
		/// </summary>
		/// <param name="key">The password, use Windows SID if null or empty</param>
		/// <returns>
		/// true if the save was successful, false otherwise
		/// </returns>
		public bool Save(string key = null)
		{
			if (string.IsNullOrWhiteSpace(key)) key = WindowsIdentity.GetCurrent().User.Value;

			serializer.Key = Encoding.UTF8.GetBytes(key);
			var r = serializer.Serialize(Root.ToDSO(), file);

			if (r) TryCount = 3;

			return r;
		}

		/// <summary>
		/// Delete the save file if any
		/// </summary>
		/// <returns>
		/// true if the file was deleted, false otherwise
		/// </returns>
		public bool DeleteSave()
		{
			try
			{
				File.Delete(file);
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}
	}
}
