
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JoinService.Models
{
    [ProtoContract]
    public class ModelliManifest
    {
        [ProtoMember(1)]
        public List<ModelliManifestItem> Items { get; set; } = new List<ModelliManifestItem>();


    }

    [ProtoContract]
    public class ModelliManifestItem
    {
        [ProtoMember(1)]
        public string FileName { get; set; }

        [ProtoMember(2)]
        public string MinAppVersion { get; set; }

        [ProtoMember(3)]
        public string Note { get; set; }

        [ProtoMember(4)]
        public List<string> Tags { get; set; }

        [ProtoMember(7)]
        public string UserName { get; set; }

    }
}