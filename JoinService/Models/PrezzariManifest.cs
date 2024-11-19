
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JoinService.Models
{
    [ProtoContract]
    public class PrezzariManifest
    {
        [ProtoMember(1)]
        public List<PrezzariManifestItem> Items { get; set; } = new List<PrezzariManifestItem>();


    }

    [ProtoContract]
    public class PrezzariManifestItem
    {
        [ProtoMember(1)]
        public string FileName { get; set; }

        [ProtoMember(2)]
        public string MinAppVersion { get; set; }

        [ProtoMember(3)]
        public string Note { get; set; }

        [ProtoMember(4)]
        public string Group { get; set; }

        [ProtoMember(5)]
        public string Year { get; set; }

    }
}