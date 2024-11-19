using System;
using System.Collections.Generic;
using System.Linq;

namespace PH.WorkingDaysAndTimeUtility.Configuration
{
    /// <summary>
    /// Representation of a Work Day.
    /// </summary>
    public class WorkDaySpan
    {
        static char _workTimeSpanSeparator = ';';

        /// <summary>
        /// Work times slices.
        /// </summary>
        public List<WorkTimeSpan> TimeSpans { get; set; } = new List<WorkTimeSpan>();

        public WorkDaySpan()
            :this(new List<WorkTimeSpan>())
        {
        }

        public WorkDaySpan(List<WorkTimeSpan> spans)
        {
            TimeSpans = spans;
        }

        public WorkDaySpan(string str)
        {
            List<WorkTimeSpan> timeSpans = GetTimeSpans(str);
            if (CheckTimeSpans(timeSpans))
                TimeSpans = timeSpans;
        }

        public string ConvertToString()
        {
            string str = string.Join(_workTimeSpanSeparator.ToString(), TimeSpans.Select(item => item.ConvertToString()));
            return str;
        }
        

        /// <summary>
        /// Get Working Minutes Per Day
        /// </summary>
        public double WorkingMinutesPerDay {
            get { return GetWorkingMinutesPerDay(); }
        }

        /// <summary>
        /// True if working day.
        /// </summary>
        public bool IsWorkingDay {
            get { return WorkingMinutesPerDay > (double)0; }
        }

        /// <summary>
        /// Cycle working-time slices and get total minutes.
        /// </summary>
        /// <returns></returns>
        private double GetWorkingMinutesPerDay()
        {
            double totalMinutes = 0;
            if (null != TimeSpans &&  TimeSpans.Count > 0)
            {
                TimeSpans.ForEach(t =>
                {
                    totalMinutes += t.GetWorkingMinutes();//t.End.Subtract(t.Start).TotalMinutes;
                });
            }
            return totalMinutes;
        }

        
        public WorkDaySpan Time(IEnumerable<WorkTimeSpan> worktimes)
        {
            TimeSpans.AddRange(worktimes);
            return this;
        }


        public WorkDaySpan Time(WorkTimeSpan w) => Time(new List<WorkTimeSpan>() {w});
        

        public WorkDaySpan Time(TimeSpan start, TimeSpan end) => Time(new WorkTimeSpan(start, end));

        public TimeSpan StartingTime
        {
            get
            {
                WorkTimeSpan wts = TimeSpans.FirstOrDefault();
                if (wts != null)
                    return wts.Start;

                return TimeSpan.Zero;
            }
        }

        public TimeSpan EndingTime
        {
            get
            {
                WorkTimeSpan wts = TimeSpans.LastOrDefault();
                if (wts != null)
                    return wts.End;

                return TimeSpan.Zero;
            }
        }

        public bool IsEndingTimeMidnight { get => EndingTime == Midnight; }

        static TimeSpan Midnight { get; } = new TimeSpan(24, 0, 0);

        public double GetWorkingMinutesBetween(TimeSpan start, TimeSpan end)
        {
            if (start >= end)
                return 0;

            double minutes = 0;
            for (int i=0; i < TimeSpans.Count; i++)
            {
                WorkTimeSpan t = TimeSpans[i];

                if (t.Start <= start && end <= t.End)
                {
                    //start e end all'interno dello stesso intervallo
                    minutes += (end - start).TotalMinutes;
                }
                else if (start <= t.Start && t.End <= end)
                {
                    //intervallo compreso interamente tra start e end
                    minutes += t.GetWorkingMinutes();// (t.End - t.Start).TotalMinutes;
                }
                else if (t.Start <= start && start <= t.End)
                {
                    //solo start dentro l'intervallo
                    minutes += t.MinutesToEnd(start);// (t.End - start).TotalMinutes;
                }
                else if (t.Start <= end && end <= t.End)
                {
                    //solo end dentro l'intervallo
                    minutes += (end - t.Start).TotalMinutes;
                }

            }
            return minutes;
        }

        public static List<WorkTimeSpan> GetTimeSpans(string str)
        {
            if (str is null || !str.Any())
                return new List<WorkTimeSpan>();

            List<string> workTimeSpans = str.Split(_workTimeSpanSeparator).ToList();
            List<WorkTimeSpan> timeSpans = workTimeSpans.Select(item => new WorkTimeSpan(item)).ToList();

            return timeSpans;
        }

        internal bool IsWorkingMoment(TimeSpan time)
        {
            if (time == GetWorkingMoment(time))
                return true;

            return false;
        }

        internal TimeSpan? GetWorkingMoment(TimeSpan time, bool forward = true)
        {
            TimeSpan? newTime = null;

            for (int i = 0; i < TimeSpans.Count; i++)
            {
                WorkTimeSpan t = TimeSpans[i];
                if (t.Start <= time && time <= t.End)
                {
                    newTime =  time;
                    break;
                }
                else if (forward)
                {
                    if (time < t.End)
                    {
                        newTime = t.Start;
                        break;
                    }
                }
                else //back
                {
                    if (time > t.Start)
                        newTime = t.End;
                }
            }
            return newTime;
        }

        //static WorkDaySpan()
        //{
        //    CheckTimeSpans("11:00;21:00");
        //}

        public static bool CheckTimeSpans(string str)
        {
            List<WorkTimeSpan> timeSpans = GetTimeSpans(str);
            return CheckTimeSpans(timeSpans);
        }

        public static bool CheckTimeSpans(List<WorkTimeSpan> timeSpans)
        {
            if (timeSpans == null)
                return false;

            WorkTimeSpan t_prec = null;
            for (int i = 0; i < timeSpans.Count; i++)
            {
                WorkTimeSpan t = timeSpans[i];

                if (t.Start >= t.End)
                    return false;

                if (t_prec != null && t_prec.End > t.Start)
                    return false;

                t_prec = t;
            }

            return true;
        }


    }
}