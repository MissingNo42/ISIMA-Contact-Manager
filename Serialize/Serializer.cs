using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Serialize
{
	/// <summary>
	/// Serializes and deserializes objects to cyphed files
	/// </summary>
	/// <typeparam name="T">The targeted type to (de)serialize</typeparam>
	public abstract class Serializer<T>
		where T : class

	{
		/// <summary>
		/// The AES init vector
		/// </summary>
		protected byte[] iv;
		private byte[] key;

		/// <summary>
		/// The AES key
		/// </summary>
		public byte[] Key
		{
			protected get => key; set => key = SHA256.Create().ComputeHash(value); // SHA512 of the key = 256 bits key = 32 bytes key
		}

		/// <summary>
		/// Creates a new serializer with the specified key and init vector
		/// </summary>
		/// <param name="key">The AES key / password</param>
		/// <param name="iv">The optional AES init vector</param>
		public Serializer(string key, string iv = null) : this(Encoding.UTF8.GetBytes("Salt#42#" + key), iv == null ? null : Encoding.UTF8.GetBytes(iv)) { }

		/// <summary>
		/// Creates a new serializer with the specified key and init vector
		/// </summary>
		/// <param name="key">The AES key / password</param>
		/// <param name="iv">The optional AES init vector</param>
		public Serializer(byte[] key, byte[] iv = null)
		{
			Key = key;
			this.iv = iv ?? MD5.Create().ComputeHash(Encoding.UTF8.GetBytes("iv")); // AES init vector is MD5 of "iv" if not provided (16 bytes)
		}

		/// <summary>
		/// Serializes an object to the specified file, cyphed with AES
		/// </summary>
		/// <param name="obj">The object to serialize</param>
		/// <param name="filePath">The destination file</param>
		/// <returns>true on success, false otherwise</returns>
		public abstract bool Serialize(T obj, string filePath);

		/// <summary>
		/// Deserializes an object from the specified file, decyphed with AES
		/// </summary>
		/// <param name="filePath">The file countaining serialized data</param>
		/// <returns>The deserialized object on success, null otherwise</returns>
		/// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
		public abstract T Deserialize(string filePath);
	}
}
