using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.JoinService
{
    public class PrezzarioInfo
    {
        public string FileName { get; set; }

        public string MinAppVersion { get; set; }

        public string Note { get; set; } = null;

        public string Group { get; set; } = null;

        public DateTime LastWriteTime { get; set; }

        public long Dimension { get; set; }

        public string Year { get; set; } = null;


    }
}
