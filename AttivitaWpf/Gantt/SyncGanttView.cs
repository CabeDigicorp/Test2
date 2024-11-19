using Syncfusion.Windows.Controls.Gantt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf.View
{
    public class SyncGanttView
    {
        public ObservableCollection<TaskDetails> Tasks { get; set; }
        public GanttHolidayCollection Holidays { get; set; }
        public Days Weekends { get; set; }
        public SyncGanttView ()
        {
            Tasks = new ObservableCollection<TaskDetails>();
            Holidays = new GanttHolidayCollection();
        }
    }
}
