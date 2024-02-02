using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DataManager
{
	/// <summary>
	/// Represents a folder, containing items
	/// </summary>
	public class Folder : Item
	{
		/// <summary>
		/// The list of items (private)
		/// </summary>
		private readonly List<Item> items = new List<Item>();

		/// <summary>
		/// The item's name
		/// </summary>
		/// <exception cref="FormatException">Raised if / or \ in the name</exception>
		/// <exception cref="DuplicateNameException">Raised if the folder name is already used</exception>
		public new string Name
		{
			get => base.Name;
			set
			{
				if (value.Contains('/') || value.Contains('\\')) throw new FormatException("The folder name cannot contain '/' or '\\'");
				var o = ((Folder)Parent)?.GetFolder(value);
				if (o != null && o != this) throw new DuplicateNameException($"A folder with the name \"{value}\" already exists");
				LastModified = DateTime.Now;
				base.Name = value;
			}
		}

		/// <summary>
		/// Create a new folder
		/// </summary>
		/// <param name="name">The folder's name</param>
		/// <exception cref="FormatException">Raised if / or \ in the name</exception>
		public Folder(string name) : base(name)
		{
			Name = name; // trigger the setter
		}

		/// <summary>
		/// Add an item to the folder
		/// </summary>
		/// <param name="item">The item to add</param>
		/// <exception cref="DuplicateNameException">Raised if the item is a folder whose name is already used by another folder</exception>"
		public void AddItem(Item item)
		{
			if (item is Folder folder && items.Any(item => item is Folder fd && fd.Name == folder.Name))
				throw new DuplicateNameException($"A folder with the name \"{folder.Name}\" already exists");

			if (item.Parent != null)
				throw new ArgumentException("The item already belongs to another folder, remove it first");

			item.Parent = this;
			LastModified = DateTime.Now;
			items.Add(item);
		}

		/// <summary>
		/// Remove a folder by name
		/// </summary>
		/// <param name="name">The folder name</param>
		/// <returns>true on success, false otherwise</returns>
		public bool RemoveFolder(string name)
		{
			var o = (Folder)items.Find(item => item is Folder fd && fd.Name == name);

			if (o == null) return false;

			o.Parent = null;

			items.Remove(o);

			LastModified = DateTime.Now;

			return true;
		}

		/// <summary>
		/// Remove contacts by name
		/// </summary>
		/// <param name="name">The name of the contact(s) to remove</param>
		/// <returns>Number of removed contacts</returns>
		public int RemoveContact(string name)
		{
			int rm = 0;

			foreach (Contact contact in items.Where(item => item is Contact && item.Name == name).ToArray())
			{
				contact.Parent = null;
				items.Remove(contact);
				rm++;
			}

			if (rm > 0) LastModified = DateTime.Now;
			return rm;
		}

		/// <summary>
		/// Remove an item from the folder
		/// </summary>
		/// <param name="item">The item to remove</param>
		/// <exception cref="ArgumentException">Raised if the item does not belong to this folder</exception>"
		public void RemoveItem(Item item)
		{
			if (item.Parent != this) throw new ArgumentException("The item does not belong to this folder");

			item.Parent = null;

			items.Remove(item);

			LastModified = DateTime.Now;
		}

		/// <summary>
		/// Get an item from the folder
		/// </summary>
		/// <param name="path">the relative path from this folder: '.' and '..' are valid, starting '/' or '\' is invalid</param>
		/// <returns>The folder if found, null otherwise</returns>
		public Folder GetFolder(string path)
		{
			string[] names = path.Split(['/', '\\'], 2); // Split only once: path/subpath/subsubpath => [path, subpath/subsubpath]
			path = names[0];
			string subpath = names.Length == 1 ? null : names[1];

			Folder folder = null;

			if (path == "." || path == "") folder = this;                                     // . and "" are valid -> current folder
			else if (path == "..") folder = (Folder)Parent;                                   // .. is valid -> parent folder
			else folder = items.Find(item => item is Folder fd && fd.Name == path) as Folder; // Find the folder with the specified name

			return subpath == null ? folder : folder?.GetFolder(subpath);                     // If subpath is null, return the folder, otherwise, continue the recursion
		}

		/// <summary>
		/// Get a contact in the folder
		/// </summary>
		/// <returns>
		/// The contact in the folder if found, null otherwise
		/// </returns>
		public Contact GetContact(string name)
		{
			return (Contact)items.FirstOrDefault(item => item is Contact && item.Name == name);
		}

		/// <summary>
		/// Get all the folders in the folder
		/// </summary>
		/// <returns>
		/// All the folders in the folder
		/// </returns>
		public IEnumerable<Folder> GetFolders()
		{
			return items.Where(item => item is Folder).Cast<Folder>();
		}

		/// <summary>
		/// Get all the contacts in the folder
		/// </summary>
		/// <returns>
		/// All the contacts in the folder
		/// </returns>
		public IEnumerable<Contact> GetContacts()
		{
			return items.Where(item => item is Contact).Cast<Contact>();
		}

		/// <summary>
		/// Get all the items in the folder
		/// </summary>
		/// <returns>
		/// All the items in the folder
		/// </returns>
		public IEnumerable<Item> GetItems()
		{
			return items.AsEnumerable();
		}

		/// <summary>
		/// Get the root folder
		/// </summary>
		/// <returns>The root folder</returns>
		public Folder GetRoot()
		{
			Folder folder = this;
			while (folder.Parent != null) folder = (Folder)folder.Parent;
			return folder;
		}

		/// <summary>
		/// Clear the folder
		/// </summary>
		public void Clear()
		{
			foreach (Item item in items) item.Parent = null;
			items.Clear();
			LastModified = DateTime.Now;
		}

		/// <summary>
		/// Get the absolute path of the folder
		/// </summary>
		/// <returns>
		/// The absolute path of the folder
		/// </returns>
		public string GetPath()
		{
			StringBuilder path = new("/");
			List<string> names = [];

			Folder folder = this;
			while (folder.Parent != null) // .parent -> avoid adding the root folder
			{
				names.Add(folder.Name);
				folder = (Folder)folder.Parent;
			}

			path.Append(string.Join("/", names.Reverse<string>().ToArray())); // Reverse the list and join it with / as separator

			return path.ToString();
		}

		/// <summary>
		/// Log the folder and its content to the console
		/// </summary>
		/// <param name="prefix">Prefix to apply (used as padding)</param>
		public override void Write(string prefix = "")
		{
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.Write($"{prefix}{Name}");
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine($"\t(création {CreationDate.ToString("G", CultureInfo.CurrentCulture)})");
			Console.ForegroundColor = ConsoleColor.White;

			prefix += "\t";
			foreach (var c in GetContacts()) c.Write(prefix); // Write contacts first

			foreach (var c in items.Where(item => item is not Folder && item is not Contact)) c.Write(prefix); // Then write other items

			foreach (var c in GetFolders()) // Then write folders
			{
				Console.WriteLine();
				c.Write(prefix);
			}
		}

		/// <summary>
		/// Export the folder to a Serializable DSO
		/// </summary>
		/// <returns>
		/// The DSO object of the folder
		/// </returns>
		public override DSO.Item ToDSO()
		{
			var list = new List<DSO.Item>();
			var dso = new DSO.Folder(Name, CreationDate, LastModified, list);

			foreach (var item in items)
			{
				list.Add(item.ToDSO());
			}

			return dso;
		}
	}
}
