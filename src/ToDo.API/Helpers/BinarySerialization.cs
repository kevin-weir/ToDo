using System.Text.Json;

namespace ToDo.API.Helpers
{
    public static class BinarySerialization<T>
    {
        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(T obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes<T>(obj);
        }

        // Convert a byte array to an Object
        public static T ByteArrayToObject(byte[] arrBytes)
        {
            var utf8Reader = new Utf8JsonReader(arrBytes);
            return JsonSerializer.Deserialize<T>(ref utf8Reader);
        }
    }
}
