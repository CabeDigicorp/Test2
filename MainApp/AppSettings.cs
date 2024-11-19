using Org.BouncyCastle.Tls;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MainApp
{
    [ProtoContract]
    public class AppSettings
    {
        /// <summary>
        /// File locale residente nella cartella decisa dall'utente
        /// </summary>
        public static string AppSettingsFileName { get => "AppSettings.ini"; }

        //[ProtoMember(1)]
        //public List<string> RecentProjectsPath_Old { get; set; } = new List<string>();

        [ProtoMember(2)]
        public string PrezzariPath { get; set; } = string.Empty;

        [ProtoMember(3)]
        public string ModelliPath { get; set; } = string.Empty;

        [ProtoMember(4)]
        public string LanguageCode { get; set; } = string.Empty;

        //[ProtoMember(5)]
        //public string UserEmail { get; set; } = string.Empty;

        //[ProtoMember(6)]
        //public string UserEncryptedPassword { get; set; } = string.Empty;

        [ProtoMember(7)]
        public List<RecentProjectInfo> RecentProjects { get; set; } = new List<RecentProjectInfo>();

        [ProtoMember(8)]
        public long CacheMaxBytes { get; set; } = 0;

        [ProtoMember(9)]
        public int AutoSaveInterval { get; set; } = 0;

        [ProtoMember(10)]
        public int ServerIndex { get; set; } = (int) WebServiceClient.ServerAddress.ServerName.Azure;//0:local, 1:pcCompile4, 2:Azure
        
        [ProtoMember(11)]
        public string AppVersion { get; set; } = string.Empty;

    }

    [ProtoContract]
    public class RecentProjectInfo
    {
        [ProtoMember(1)]
        public string Path { get; set; } = string.Empty;

        [ProtoMember(2)]
        public byte[] Thumbnail
        {
            get;
            set;
        } = null;

        [ProtoMember(3)]
        public ProjectSourceType ProjectSourceType { get; set; } = ProjectSourceType.File;

        [ProtoMember(4)]
        public Guid OperaId { get; set; } = Guid.Empty;

        [ProtoMember(5)]
        public Guid ProgettoId { get; set; } = Guid.Empty;
    }

}
