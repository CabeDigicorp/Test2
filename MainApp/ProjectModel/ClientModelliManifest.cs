using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainApp
{
    [ProtoContract]
    public class ClientModelliManifest
    {
        [ProtoMember(1)]
        public List<ClientModelliManifestItem> Items { get; set; } = new List<ClientModelliManifestItem>();


    }

    [ProtoContract]
    public class ClientModelliManifestItem
    {
        [ProtoMember(1)]
        public string FileName { get; set; }

        [ProtoMember(2)]
        public string Note { get; set; }

        [ProtoMember(3)]
        public List<string> Tags { get; set; }

        [ProtoMember(4)]
        public DateTime DownloadDate { get; set; }

        [ProtoMember(5)]
        public DateTime ServiceLastWriteTime { get; set; }

        [ProtoMember(6)]
        public string MinAppVersion { get; set; }

        [ProtoMember(7)]
        public string UserName { get; set; }

    }
}
