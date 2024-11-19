using CommonResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class WeekHours
    {
        public List<WeekDay> Days = new List<WeekDay>();

        public WeekHours()
        {
        }

        static public WeekHours Default
        {
            get
            {
                WeekHours weekHours = new WeekHours();
                //string orarioDefault = "8:30-12:30;14:00-18:00";
                string orarioDefault = "0:00-24:00";

                weekHours.Days.Add(new WeekDay() { Id = DayOfWeek.Monday, Hours = orarioDefault });
                weekHours.Days.Add(new WeekDay() { Id = DayOfWeek.Tuesday, Hours = orarioDefault });
                weekHours.Days.Add(new WeekDay() { Id = DayOfWeek.Wednesday, Hours = orarioDefault });
                weekHours.Days.Add(new WeekDay() { Id = DayOfWeek.Thursday, Hours = orarioDefault });
                weekHours.Days.Add(new WeekDay() { Id = DayOfWeek.Friday, Hours = orarioDefault });
                weekHours.Days.Add(new WeekDay() { Id = DayOfWeek.Saturday, Hours = orarioDefault });
                weekHours.Days.Add(new WeekDay() { Id = DayOfWeek.Sunday, Hours = orarioDefault });

                return weekHours;
            }

        }

        public string ToUserText()
        {
            List<string> settimana = new List<string>() { "Domenica", "Lunedi", "Martedi", "Mercoledi", "Giovedi", "Venerdi", "Sabato" };
            string weekHoursText = string.Empty;
            Days.ForEach(item => weekHoursText = string.Format("{0}{1} {2}\n", weekHoursText, LocalizationProvider.GetString(settimana[(int)item.Id]), item.Hours));
            return weekHoursText;
        }
    }

    public class WeekDay
    {
        public DayOfWeek Id { get; set; }
        public string Hours { get; set; } = string.Empty; //8:30-12:30;14:00-18:00
    }

    public class CustomDays
    {
        public List<CustomDay> Days { get; set; } = new List<CustomDay>();
    }

    public class CustomDay
    {
        public DateTime Day { get; set; } = DateTime.MinValue;
        public string Hours { get; set; } = string.Empty;//8:30-12:30;14:00-18:00
    }
    
}
