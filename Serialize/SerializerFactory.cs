using System;

namespace Serialize
{
	/// <summary>
	/// Available serializer types
	/// </summary>
	public enum SerializerType
	{
		/// <summary>
		/// XML serializer
		/// </summary>
		XML,

		/// <summary>
		/// Binary serializer (deprecated)
		/// </summary>
		Binary,
	}

	/// <summary>
	/// Serializer factory singleton class
	/// </summary>
	public static class SerializerFactory
	{
		/// <summary>
		/// Get a serializer of the selected type, targeting the type T
		/// </summary>
		/// <typeparam name="T">The returned serializer targeted type</typeparam>
		/// <param name="type">The type of the returned serializer</param>
		/// <param name="key">the initial password used for the AES encryption by the returned serializer</param>
		/// <returns>The T serializer object</returns>
		/// <exception cref="ArgumentException">thrown on invalid serializer type</exception>
		public static Serializer<T> GetSerializer<T>(SerializerType type, string key)
			where T : class
		{
			switch (type)
			{
				case SerializerType.XML:
					return new XmlSerializer<T>(key);
				case SerializerType.Binary:
					return new BinarySerializer<T>(key);
				default:
					throw new ArgumentException("Invalid serializer type");
			}
		}
	}
}
