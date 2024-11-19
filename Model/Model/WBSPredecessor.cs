using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class WBSPredecessors
    {
        public List<WBSPredecessor> Items{ get; set; } = new List<WBSPredecessor>();

    }

    public class WBSPredecessor
    {
        public Guid WBSItemId { get; set; } = Guid.Empty;
        public WBSPredecessorType Type { get; set; } = WBSPredecessorType.FinishToStart;
        public double DelayDays { get; set; } = 0;
        public bool DelayFixed { get; set; } = false;
    }

    public enum WBSPredecessorType
    {
        FinishToStart = 0,
        FinishToFinish = 1,
        StartToStart = 2,
        StartToFinish = 3,
    }

}
