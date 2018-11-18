namespace RssReader.Library
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;

    public class Base64Gzip: ITypeConverter
    {
        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            string text = value as string;
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(text);
            using (var memoryStream = new MemoryStream())
            {
                using (GZipStream zipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    zipStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            var outputStream = new MemoryStream();
            var plainTextBytes = Convert.FromBase64String(text);
            using (var memoryStream = new MemoryStream(plainTextBytes))
            {
                using (GZipStream zipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    zipStream.CopyTo(outputStream);
                }

                return outputStream.ToArray().ToString();
            }
        }
    }
}
