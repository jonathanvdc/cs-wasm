using System.Collections.Generic;
using System.Text;

namespace Wasm.Binary
{
    /// <summary>
    /// /// A string that is encoded as a byte array.
    /// </summary>
    public struct ByteString
    {
        /// <summary>
        /// Creates a byte string from the given data.
        /// </summary>
        /// <param name="Data">The data for this byte string.</param>
        public ByteString(byte[] Data)
        {
            this.byteArray = Data;
        }

        private byte[] byteArray;

        /// <summary>
        /// Gets a Boolean flag that tells if this byte string is the null byte string.
        /// </summary>
        public bool IsNullByteString => byteArray == null;

        /// <summary>
        /// Gets this byte string's underlying byte array.
        /// </summary>
        /// <returns>The underlying byte array.</returns>
        public byte[] Bytes => byteArray;

        /// <summary>
        /// Gets this byte string's data as a UTF-8 string.
        /// </summary>
        /// <returns>A string.</returns>
        public string Utf8String 
        {
            get
            {
                if (IsNullByteString)
                    return null;
                else
                    return UTF8Encoding.UTF8.GetString(byteArray);
            }
        }

        public override string ToString()
        {
            return Utf8String;
        }
    }
}