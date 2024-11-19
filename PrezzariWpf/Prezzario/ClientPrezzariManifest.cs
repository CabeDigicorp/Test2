using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrezzariWpf.Prezzario
{
    [ProtoContract]
    public class ClientPrezzariManifest
    {
        [ProtoMember(1)]
        public List<ClientPrezzariManifestItem> Items { get; set; } = new List<ClientPrezzariManifestItem>();


    }

    [ProtoContract]
    public class ClientPrezzariManifestItem
    {
        [ProtoMember(1)]
        public string FileName { get; set; }

        [ProtoMember(2)]
        public string Note { get; set; }

        [ProtoMember(3)]
        public string Group { get; set; }

        [ProtoMember(4)]
        public DateTime DownloadDate { get; set; }

        [ProtoMember(5)]
        public DateTime ServiceLastWriteTime { get; set; }

        [ProtoMember(6)]
        public string Year { get; set; }

    }
}
