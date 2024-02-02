using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace DataManager.DSO
{
	/// <summary>
	/// Data Storage Object for abstract Item
	/// </summary>
	[XmlInclude(typeof(Contact))]
	[XmlInclude(typeof(Folder))]
	[Serializable]
	public abstract class Item // Item copy without ascending parent and with public getters and setters for XML serialization
	{
		public string Name { get; set; }
		public DateTime CreationDate { get; set; }
		public DateTime LastModified { get; set; }

		public Item(string name, DateTime creationDate, DateTime lastModified)
		{
			Name = name;
			CreationDate = creationDate;
			LastModified = lastModified;
		}

		public Item() { } // For serialization

		/// <summary>
		/// Instantiate an Item from the DSO
		/// </summary>
		/// <returns>The item</returns>
		public abstract DataManager.Item ToObject();
	}

	/// <summary>
	/// Data Storage Object for Contact
	/// </summary>
	[Serializable]
	public class Contact : Item
	{
		public string FirstName { get; set; }
		public string Company { get; set; }
		public string MailAddress { get; set; }
		public Relation Relation { get; set; }

		public Contact(string name, DateTime creationDate, DateTime lastModified, string firstName, string company, Relation relation, string mailAddress) : base(name, creationDate, lastModified)
		{
			FirstName = firstName;
			Company = company;
			MailAddress = mailAddress;
			Relation = relation;
		}

		public Contact() { } // For serialization

		/// <summary>
		/// Instantiate a Contact from the DSO
		/// </summary>
		/// <returns>The contact</returns>
		public override DataManager.Item ToObject()
		{
			var contact = new DataManager.Contact(Name, FirstName, Company, Relation, MailAddress);
			contact.CreationDate = CreationDate;
			contact.LastModified = LastModified;

			return contact;
		}
	}

	/// <summary>
	/// Data Storage Object for Folder
	/// </summary>
	[Serializable]
	public class Folder : Item
	{
		public List<Item> Items { get; set; }

		public Folder(string name, DateTime creationDate, DateTime lastModified, List<Item> items) : base(name, creationDate, lastModified)
		{
			Items = items;
		}

		public Folder() { } // For serialization

		/// <summary>
		/// Instantiate a Folder from the DSO
		/// </summary>
		/// <returns>The folder</returns>
		public override DataManager.Item ToObject()
		{
			var folder = new DataManager.Folder(Name);

			foreach (var item in Items)
			{
				folder.AddItem(item.ToObject());
			}

			folder.CreationDate = CreationDate;
			folder.LastModified = LastModified;

			return folder;
		}
	}
}
