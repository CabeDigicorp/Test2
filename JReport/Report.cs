using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JReport
{
    public class Report : FastReport.Report
    {
        public int ContatorePagine { get; set; }
        public Report() : base()
        {
            
        }
    }
}
