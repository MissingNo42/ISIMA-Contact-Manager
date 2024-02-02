using System;

namespace DataManager
{
	/// <summary>
	/// Represents an abstract item/file
	/// </summary>
	public abstract class Item
	{
		/// <summary>
		/// The item's name (protected)
		/// </summary>
		protected string name;

		/// <summary>
		/// The item's name
		/// </summary>
		/// <exception cref="ArgumentException">Raised if null or empty</exception>
		public string Name
		{
			get => name;
			set
			{
				if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("The name cannot be empty");
				LastModified = DateTime.Now;
				name = value;
			}
		}

		/// <summary>
		/// The parent item (can be null). Set by the parent itself
		/// </summary>
		public Item Parent { get; internal protected set; }

		/// <summary>
		/// The item's creation date
		/// </summary>
		public DateTime CreationDate { get; internal protected set; } // internal -> DSO

		/// <summary>
		/// The item's last modification date
		/// </summary>
		public DateTime LastModified { get; internal protected set; } // internal -> DSO

		/// <summary>
		/// Initialize a new item
		/// </summary>
		/// <param name="name">The item's name</param>
		/// <exception cref="ArgumentException">Raised if name is null or empty</exception>
		public Item(string name)
		{
			Name = name;
			CreationDate = DateTime.Now;
			LastModified = DateTime.Now;
		}

		/// <summary>
		/// Write the item's content to the console
		/// </summary>
		/// <param name="prefix">Prefix to apply</param>
		public abstract void Write(string prefix = "");

		/// <summary>
		/// Export the item to a Serializable DSO
		/// </summary>
		/// <returns>
		/// The DSO object
		/// </returns>
		public abstract DSO.Item ToDSO();
	}
}
