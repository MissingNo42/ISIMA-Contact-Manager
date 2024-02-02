using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace Serialize
{
#pragma warning disable SYSLIB0011 // BinaryFormatter deprecated
	/// <summary>
	/// Serializes and deserializes objects to cyphed files using BinaryFormatter (deprecated) API
	/// </summary>
	/// <typeparam name="T">Type to serialize</typeparam>
	internal class BinarySerializer<T> : Serializer<T>
		where T : class
	{
		/// <summary>
		/// Creates a new serializer with the specified key and init vector
		/// </summary>
		/// <param name="key">The AES key / password</param>
		/// <param name="iv">The optional AES init vector</param>
		public BinarySerializer(string key, string iv = null) : base(key, iv) { }

		/// <summary>
		/// Creates a new serializer with the specified key and init vector
		/// </summary>
		/// <param name="key">The AES key / password</param>
		/// <param name="iv">The optional AES init vector</param>
		public BinarySerializer(byte[] key, byte[] iv = null) : base(key, iv) { }

		/// <summary>
		/// Serializes an object to the specified file, cyphed with AES
		/// </summary>
		/// <param name="obj">The object to serialize</param>
		/// <param name="filePath">The destination file</param>
		/// <returns>true on success, false otherwise</returns>
		public override bool Serialize(T obj, string filePath)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(filePath)!); // Create the directory if it doesn't exist

				using Stream fs = new FileStream(filePath, FileMode.Create),
							  cs = new CryptoStream(fs, Aes.Create().CreateEncryptor(Key, iv), CryptoStreamMode.Write);
				new BinaryFormatter().Serialize(cs, obj!);
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Deserializes an object from the specified file, decyphed with AES
		/// </summary>
		/// <param name="filePath">The file countaining serialized data</param>
		/// <returns>The deserialized object on success, null otherwise</returns>
		/// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
		public override T Deserialize(string filePath)
		{
			try
			{
				using Stream fs = new FileStream(filePath, FileMode.Open),
							  cs = new CryptoStream(fs, Aes.Create().CreateDecryptor(Key, iv), CryptoStreamMode.Read);
				return (T)new BinaryFormatter().Deserialize(cs);
			}
			catch (FileNotFoundException)
			{
				throw;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
#pragma warning restore SYSLIB0011
}
