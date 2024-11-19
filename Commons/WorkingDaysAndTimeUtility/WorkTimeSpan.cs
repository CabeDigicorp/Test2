using System;

namespace PH.WorkingDaysAndTimeUtility.Configuration
{
    /// <summary>
    /// A Slice of work-time.
    /// </summary>
    public class WorkTimeSpan
    {
        char _startEndSeparator = '-';
        char _hourMinSeparator = ':';

        /// <summary>
        /// Starting Time for Work
        /// </summary>
        public TimeSpan Start { get; set; } = TimeSpan.MinValue;
        /// <summary>
        /// End Time for Work
        /// </summary>
        public TimeSpan End { get; set; } = TimeSpan.MinValue;

        public WorkTimeSpan()
        {
            
        }

        public WorkTimeSpan(TimeSpan start, TimeSpan end)
        {
            Start = start;
            End = end;
        }

        //by Ale
        public WorkTimeSpan(string str)
        {



            int h = -1;
            int min = -1;

            //start
            string[] start_end = str.Split(_startEndSeparator);

            if (start_end.Length < 2)
                return;

            string start = start_end[0];
            string[] h_m = start.Split(_hourMinSeparator);

            if (h_m.Length < 2)
                return;

            int.TryParse(h_m[0], out h);
            int.TryParse(h_m[1], out min);

            if (h < 0 || h > 23)
                return;

            if (min < 0 || min > 59)
                return;

            Start = new TimeSpan(h, min, 0);

            //end
            string end = start_end[1];
            h_m = end.Split(_hourMinSeparator);

            if (h_m.Length < 2)
                return;

            int.TryParse(h_m[0], out h);
            int.TryParse(h_m[1], out min);

            if (h == 24)
            {
                //End = TimeSpan.MaxValue;
                End = new TimeSpan(24, 0, 0);
            }
            else
            {

                if (h < 0 || h > 23)
                    return;

                if (min < 0 || min > 59)
                    return;

                End = new TimeSpan(h, min, 0);
            }
        }


        public string ConvertToString()
        {
            string start = string.Join(_hourMinSeparator.ToString(), Start.Hours, Start.Minutes);
            string end = string.Join(_hourMinSeparator.ToString(), End.Hours, End.Minutes);

            string str = string.Join(_startEndSeparator.ToString(), start, end);
            return str;
        }

        public double GetWorkingMinutes()
        {
            return MinutesToEnd(Start);
        }

        internal double MinutesToEnd(TimeSpan start)
        {
            if (End == TimeSpan.MaxValue)
            {
                TimeSpan ts = TimeSpan.Zero - start;
                double mins = (24.0 * 60) + ts.TotalMinutes;
                return mins;
            }
            else
            {
                var res = End - start;
                return res.TotalMinutes;
            }
        }
    }
}