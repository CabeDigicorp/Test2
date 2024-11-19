using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace JoinWebUI.Utilities
{
    public class FileHandler
    {
        public string FileName { get; set; }
        public string FileContentType { get; set; }
        public byte[] FileContent { get; set; }
    }

    public class FileChunkedHandler
    {
        public string FileName { get; set; }
        public string FileContentType { get; set; }
        public List<byte[]> FileChunkedContent { get; set; }
    }

    public class Serialization
    {

     
    }
}

