using System;
using System.ComponentModel.DataAnnotations;

namespace DataManager
{
	/// <summary>
	/// Represents contacts relations
	/// </summary>
	public enum Relation
	{
		Friend = 0,
		Colleague,
		Network,
		Other
	}

	/// <summary>
	/// Represents a contact
	/// </summary>
	public class Contact : Item
	{
		/// <summary>
		/// The contact's first name (private)
		/// </summary>
		private string firstName;

		/// <summary>
		/// The contact's company (private)
		/// </summary>
		private string company;

		/// <summary>
		/// The contact's mail address (private)
		/// </summary>
		private string mailAddress;

		/// <summary>
		/// The contact's relation (private)
		/// </summary>
		private Relation relation;

		/// <summary>
		/// The contact's first name
		/// </summary>
		/// <exception cref="ArgumentException">Raised if first name is null or empty</exception>
		public string FirstName
		{
			get => firstName;
			set
			{
				if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("The first name cannot be empty");
				LastModified = DateTime.Now;
				firstName = value;
			}
		}

		/// <summary>
		/// The contact's company
		/// </summary>
		/// <exception cref="ArgumentException">Raised if company is null or empty</exception>
		public string Company
		{
			get => company;
			set
			{
				if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("The company name cannot be empty");
				LastModified = DateTime.Now;
				company = value;
			}
		}

		/// <summary>
		/// The contact's relation
		/// </summary>
		public Relation Relation
		{
			get => relation;
			set
			{
				LastModified = DateTime.Now;
				relation = value;
			}
		}

		/// <summary>
		/// The contact's mail address
		/// </summary>
		/// <exception cref="FormatException">Raised if not a valid email address</exception>
		public string MailAddress
		{
			get => mailAddress;
			set
			{
				if (!new EmailAddressAttribute().IsValid(value)) throw new FormatException($"\"{value}\" is not a valid email address"); ;
				LastModified = DateTime.Now;
				mailAddress = value;
			}
		}

		/// <summary>
		/// Create a new contact
		/// </summary>
		/// <param name="name">The contact's last name</param>
		/// <param name="firstName">The contact's first name</param>
		/// <param name="company">The contact's company</param>
		/// <param name="relation">The contact's relation</param>
		/// <param name="mailAddress">The contact's mail address</param>
		/// <exception cref="FormatException">Raised if not a valid email address</exception>
		/// <exception cref="ArgumentException">Raised if name, first name or company are null or empty</exception>
		public Contact(string name, string firstName, string company, Relation relation, string mailAddress) : base(name)
		{
			FirstName = firstName;
			Company = company;
			Relation = relation;
			MailAddress = mailAddress;
		}


		/// <summary>
		/// Log the contact to the console
		/// </summary>
		/// <param name="prefix">Prefix to apply (used as padding)</param>
		public override void Write(string prefix = "")
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write($"{prefix}-\t{FirstName}\t{Name}");

			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.Write($"\t{Relation.ToString()}");

			Console.ForegroundColor = ConsoleColor.White;
			Console.Write($" at ");

			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.Write($"{Company}");

			Console.ForegroundColor = ConsoleColor.White;
			Console.Write($": ");

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"{MailAddress}");

			Console.ForegroundColor = ConsoleColor.White;
		}

		/// <summary>
		/// Export the contact to a Serializable DSO
		/// </summary>
		/// <returns>
		/// The DSO object of the contact
		/// </returns>
		public override DSO.Item ToDSO()
		{
			return new DSO.Contact(Name, CreationDate, LastModified, FirstName, Company, Relation, MailAddress);
		}
	}
}
