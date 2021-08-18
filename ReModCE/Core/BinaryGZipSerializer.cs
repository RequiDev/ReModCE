using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MelonLoader.ICSharpCode.SharpZipLib.GZip;

namespace ReModCE.Core
{
    internal class BinaryGZipSerializer
    {
        public static void Serialize(object value, string path)
        {
            var formatter = new BinaryFormatter();

            using (var fStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var gzipStream = new GZipOutputStream(fStream))
                {
                    formatter.Serialize(gzipStream, value);
                }
            }
        }
        
        public static object Deserialize(string path)
        {
            var formatter = new BinaryFormatter();

            using (Stream fStream = File.OpenRead(path))
            {
                using (var gzipStream = new GZipInputStream(fStream))
                {
                    return formatter.Deserialize(gzipStream);
                }
            }
        }
    }
}
