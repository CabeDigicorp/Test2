using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoinService.Models
{
    public class ModelloInfo
    {
        public string FileName { get; set; }//remote file name

        public string MinAppVersion { get; set; }

        public string Note { get; set; } = null;

        public List<string> Tags { get; set; } = new List<string>();

        public DateTime LastWriteTime { get; set; }

        public long Dimension { get; set; }

        public string UserName { get; set; }//nome del modello visualizzato nell'interfaccia utente

        
    }
}
