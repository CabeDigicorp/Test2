
using MasterDetailModel;
using PH.WorkingDaysAndTimeUtility;
using PH.WorkingDaysAndTimeUtility.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class DateTimeCalculator
    {
        WorkingDaysAndTimeUtility _utility = null;

        public DateTime AddWorkingMinutes(DateTime start, double minutes)
        {
            

            DateTime res = _utility.AddWorkingMinutes(start, minutes);

            double mins = GetWorkingMinutesBetween(start, res);
            return res;
        }

        public DateTime AddWorkingDays(DateTime start, double days)
        {
            //if (days <= 0)
            //    return start;

            //DateTime res = _utility.AddWorkingDays(start, (int)days);//only forwards
            DateTime res = _utility.AddWorkingDays(start, (int)days);
            double minLastDay = (days - (int)days) * GetWorkingMinutesPerDay(res);
            res = _utility.AddWorkingMinutes(res, minLastDay);
            return res;
        }


        public DateTimeCalculator(WeekHours weekHours, CustomDays customDays)
        {
            if (weekHours == null)
                return;


            var week = new WeekDaySpan();
            week.WorkDays = new Dictionary<DayOfWeek, WorkDaySpan>();
            foreach (WeekDay day in weekHours.Days)
            {
                string dayHours = string.Empty;
                if (day.Hours != null)
                    dayHours = day.Hours;

                WorkDaySpan daySpan = new WorkDaySpan(dayHours);
                week.WorkDays.Add(day.Id, daySpan);
            }

            Dictionary<string, WorkDaySpan> customWorkDays = new Dictionary<string, WorkDaySpan>();
            foreach (var customDay in customDays.Days)
            {
                customWorkDays.Add(customDay.Day.ToShortDateString(), new WorkDaySpan(customDay.Hours));
            }
            

            _utility = new WorkingDaysAndTimeUtility(week, customWorkDays);

            
        }

        public bool IsValid()
        {
            if (_utility == null)
                return false;

            return true;
        }

        public bool IsWorkingMoment(DateTime dateTime)
        {
            if (!IsValid())
                return false;

            return _utility.IsWorkingMoment(dateTime);
        }

        public DateTime GetStartingDateTimeOfDay(DateTime date)
        {
            WorkDaySpan workDaySpan = _utility.GetWorkDaySpan(date);
            DateTime dateComponent = date.Date;
            DateTime res = dateComponent + workDaySpan.StartingTime;
            return res;
        }

        public DateTime GetEndingDateTimeOfDay(DateTime date)
        {
            WorkDaySpan workDaySpan = _utility.GetWorkDaySpan(date);
            DateTime dateComponent = date.Date;
            DateTime res = dateComponent + workDaySpan.EndingTime;
            return res;
        }

        public TimeSpan GetEndingTimeOfDay(DateTime date)
        {
            DateTime endOfDay = GetEndingDateTimeOfDay(date);
            if (endOfDay.Date > date.Date)
                return new TimeSpan(24, 0, 0);
            else
                return endOfDay.TimeOfDay;

        }

        

        public DateTime GetPreviousWorkingDay(DateTime dateTime)
        {
            int maxDays = 100000;
            int i = 1;
            DateTime res = dateTime;

            if (!IsValid())
                return res;

            WorkDaySpan workDaySpan = _utility.GetWorkDaySpan(dateTime);
            //HashSet<DayOfWeek> daysOfWeek = _utility.GetWorkingDays();

            while (i < maxDays)
            {
                res = dateTime.AddDays(-i);
                workDaySpan = _utility.GetWorkDaySpan(res);
                if (workDaySpan.IsWorkingDay)
                {
                    if (!workDaySpan.IsEndingTimeMidnight)
                        res = res.Date + workDaySpan.EndingTime;
                    break;
                }

                //if (daysOfWeek.Contains(res.DayOfWeek))
                //    break;

                i++;
            }

            return res;
        }


        public DateTime GetNextWorkingDay(DateTime dateTime)
        {
            int maxDays = 100000;
            int i = 1;
            DateTime res = dateTime;

            if (!IsValid())
                return res;

            WorkDaySpan workDaySpan = _utility.GetWorkDaySpan(dateTime);
            //HashSet<DayOfWeek> daysOfWeek = _utility.GetWorkingDays();

            while (i < maxDays)
            {
                res = dateTime.AddDays(i);
                workDaySpan = _utility.GetWorkDaySpan(res);

                if (workDaySpan.IsWorkingDay)
                {
                    res = res.Date + workDaySpan.StartingTime;
                    break;
                }

                i++;
            }

            return res;
        }

        public DateTime AsEndingDateTime(DateTime dateTime)
        {

            DateTime res = dateTime;

            if (!IsValid())
                return res;


            WorkDaySpan workDaySpan = _utility.GetWorkDaySpan(res);

            for (int i = 0; i < workDaySpan.TimeSpans.Count; i++)
            {
                if (workDaySpan.TimeSpans[i].Start == dateTime.TimeOfDay)
                {
                    if (i > 0)
                    {
                        res = dateTime.Date + workDaySpan.TimeSpans[i - 1].End;
                        return res;
                    }
                    else if (i==0)
                    {
                        res = GetPreviousWorkingDay(res);
                        res = GetEndingDateTimeOfDay(res);
                    }
                }
            }

            return res;
        }

        public DateTime AsStartingDateTime(DateTime dateTime)
        {
            DateTime res = dateTime;

            if (!IsValid())
                return res;

            WorkDaySpan workDaySpan = _utility.GetWorkDaySpan(res);

            for (int i = 0; i < workDaySpan.TimeSpans.Count; i++)
            {
                if (workDaySpan.TimeSpans[i].End == dateTime.TimeOfDay)
                {
                    if (i + 1 < workDaySpan.TimeSpans.Count)
                    {
                        res = dateTime.Date + workDaySpan.TimeSpans[i + 1].Start;
                        return res;
                    }
                    else if (i + 1 == workDaySpan.TimeSpans.Count)
                    {
                        res = GetNextWorkingDay(res);
                        res = GetStartingDateTimeOfDay(res);
                    }
                }
            }

            return res;
        }

        public double GetWorkingMinutesPerDay(DateTime dateTime)
        {
            if (!IsValid())
                return -1;

            return _utility.GetWorkingMinutesPerDay(dateTime);
        }

        public double GetWorkingMinutesBetween(DateTime Start, DateTime End)
        {
            if (!IsValid())
                return -1;

            DateTime start = Start;
            DateTime end = End;
            bool isForward = true;

            if (start > end)
            {
                start = End;
                end = Start;
                isForward = false;
            }

            double minutes = 0;

            WorkDaySpan startWorkDaySpan = _utility.GetWorkDaySpan(start);
            WorkDaySpan endWorkDaySpan = _utility.GetWorkDaySpan(end);

            if (start.Date == end.Date)
            {
                minutes += startWorkDaySpan.GetWorkingMinutesBetween(start.TimeOfDay, end.TimeOfDay);
            }
            else
            {
                minutes += startWorkDaySpan.GetWorkingMinutesBetween(start.TimeOfDay, GetEndingTimeOfDay(start) /*GetEndingDateTimeOfDay(start).TimeOfDay*/);
                minutes += endWorkDaySpan.GetWorkingMinutesBetween(GetStartingDateTimeOfDay(end).TimeOfDay, end.TimeOfDay);
            }


            List<DateTime> dates = _utility.GetWorkingDaysBetweenTwoDateTimes(start, end);
            foreach (DateTime date in dates)
                minutes += GetWorkingMinutesPerDay(date);

            if (!isForward)
                minutes = -minutes;

            return minutes;
        }

        public double GetWorkingDaysBetween(DateTime Start, DateTime End)
        {
            if (!IsValid())
                return -1;

            DateTime start = Start;
            DateTime end = End;
            bool isForward = true;

            if (start > end)
            {
                start = End;
                end = Start;
                isForward = false;
            }

            if (!_utility.IsWorkingMoment(start))
                start = _utility.GetNextWorkingMoment(start);

            if (!_utility.IsWorkingMoment(end))
                end = _utility.GetPrevWorkingMoment(end);

            double days = 0;
            double minutes = 0;

            WorkDaySpan startWorkDaySpan = _utility.GetWorkDaySpan(start);
            WorkDaySpan endWorkDaySpan = _utility.GetWorkDaySpan(end);

            if (start.Date == end.Date)
            {
                minutes = startWorkDaySpan.GetWorkingMinutesBetween(start.TimeOfDay, end.TimeOfDay);
                days += minutes / startWorkDaySpan.WorkingMinutesPerDay;
            }
            else
            {
                minutes = startWorkDaySpan.GetWorkingMinutesBetween(start.TimeOfDay, GetEndingTimeOfDay(start) /*GetEndingDateTimeOfDay(start).TimeOfDay*/);
                if (startWorkDaySpan.WorkingMinutesPerDay > 0)
                    days += minutes / startWorkDaySpan.WorkingMinutesPerDay;

                minutes = endWorkDaySpan.GetWorkingMinutesBetween(GetStartingDateTimeOfDay(end).TimeOfDay, end.TimeOfDay);
                if (endWorkDaySpan.WorkingMinutesPerDay > 0)
                    days += minutes / endWorkDaySpan.WorkingMinutesPerDay;
            }


            List<DateTime> dates = _utility.GetWorkingDaysBetweenTwoDateTimes(start, end);
            foreach (DateTime date in dates)
                days += 1;

            if (!isForward)
                days = -days;

            return days;

        }

        public DateTime GetNextWorkingMoment(DateTime dateTime)
        {
            return _utility.GetNextWorkingMoment(dateTime);
        }
        public DateTime GetPrevWorkingMoment(DateTime dateTime)
        {
            return _utility.GetPrevWorkingMoment(dateTime);
        }

        public DateTime? GetStartingDateTimeOfYear(DateTime date)
        {
            DateTime tmp = new DateTime(date.Year, 1, 1);
            while (!_utility.IsWorkingDay(tmp))
            {
                tmp = tmp.AddDays(1);
                if (tmp.Year != date.Year)
                    return null;
            }
            return GetStartingDateTimeOfDay(tmp);
        }

        public DateTime? GetEndingDateTimeOfYear(DateTime date)
        {
            DateTime tmp = new DateTime(date.Year, 12, 31);
            while (!_utility.IsWorkingDay(tmp))
            {
                tmp = tmp.AddDays(-1);
                if (tmp.Year != date.Year)
                    return null;
            }
            return GetEndingDateTimeOfDay(tmp);
        }

        public DateTime? GetStartingDateTimeOfMonth(DateTime date)
        {
            DateTime tmp = new DateTime(date.Year, date.Month, 1);
            while (!_utility.IsWorkingDay(tmp))
            {
                tmp = tmp.AddDays(1);
                if (tmp.Month != date.Month)
                    return null;
            }

            return GetStartingDateTimeOfDay(tmp);
        }

        public DateTime? GetEndingDateTimeOfMonth(DateTime date)
        {
            DateTime tmp = new DateTime(date.Year, date.Month, 31);
            while (!_utility.IsWorkingDay(tmp))
            {
                tmp = tmp.AddDays(-1);
                
                if (tmp.Month != date.Month)
                    return null;
            }
            return GetEndingDateTimeOfDay(tmp);
        }

        public DateTime? GetStartingDateTimeOfQuarter(DateTime date)
        {
            int quad = (int) ((date.Month / 4.0) + 1);
            int firstQuadMonth = (quad * 4) - 3;
            DateTime tmp = new DateTime(date.Year, firstQuadMonth, 1);

            while (!_utility.IsWorkingDay(tmp))
            {
                tmp = tmp.AddDays(1);
                int quadTmp = (int)((tmp.Month / 4.0) + 1);
                if (quadTmp != quad)
                    return null;
            }

            return GetStartingDateTimeOfDay(tmp);
        }

        public DateTime? GetEndingDateTimeOfQuarter(DateTime date)
        {
            int quad = (int)((date.Month / 4.0) + 1);
            int lastQuadMonth = quad * 4;
            DateTime tmp = new DateTime(date.Year, lastQuadMonth, -1);

            while (!_utility.IsWorkingDay(tmp))
            {
                tmp = tmp.AddDays(-1);
                int quadTmp = (int)((tmp.Month / 4.0) + 1);
                if (quadTmp != quad)
                    return null;
            }

            return GetEndingDateTimeOfDay(tmp);
        }

        public DateTime? GetStartingDateTimeOfWeek(DateTime date)
        {
            DateTime tmp = date;
            while (tmp.DayOfWeek != DayOfWeek.Monday)
                tmp = tmp.AddDays(-1);
            
            while (!_utility.IsWorkingDay(tmp))
            {
                tmp = tmp.AddDays(1);
                if (tmp.DayOfWeek == DayOfWeek.Monday)
                    return null;
            }

            return GetStartingDateTimeOfDay(tmp);
        }

        public DateTime? GetEndingDateTimeOfWeek(DateTime date)
        {
            DateTime tmp = date;
            while (tmp.DayOfWeek != DayOfWeek.Sunday)
                tmp = tmp.AddDays(1);

            while (!_utility.IsWorkingDay(tmp))
            {
                tmp = tmp.AddDays(-1);
                if (tmp.DayOfWeek == DayOfWeek.Sunday)
                    return null;
            }

            return GetEndingDateTimeOfDay(tmp);
        }

        public DateTime? GetStartingDateTimeOfHour(DateTime date)
        {
            DateTime hourStart = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
            DateTime hourEnd = new DateTime(date.Year, date.Month, date.Day, date.Hour + 1, 0, 0);
            DateTime tmp = hourStart;
            while (!_utility.IsWorkingMoment(tmp))
            {
               tmp = GetNextWorkingMoment(tmp);
                if (tmp >= hourEnd)
                    return null;
            }

            return AsStartingDateTime(tmp);
        }

        public DateTime? GetEndingDateTimeOfHour(DateTime date)
        {
            DateTime hourStart = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
            DateTime hourEnd = new DateTime(date.Year, date.Month, date.Day, date.Hour + 1, 0, 0);
            DateTime tmp = hourEnd;
            while (!_utility.IsWorkingMoment(tmp))
            {
                tmp = GetPrevWorkingMoment(tmp);
                if (tmp <= hourStart)
                    return null;
            }

            return AsEndingDateTime(tmp);
        }


    }
}



/*
 Code Examples

AddWorkingDays(DateTime start, int days)

//this is the configuration of a work-week: 8h/day from monday to friday
var wts1 = new WorkTimeSpan() 
	{ Start = new TimeSpan(9, 0, 0), End = new TimeSpan(13, 0, 0) };
var wts2 = new WorkTimeSpan() 
	{ Start = new TimeSpan(14, 0, 0), End = new TimeSpan(18, 0, 0) };
var wts = new List<WorkTimeSpan>() { wts1, wts2 };

var week = new WeekDaySpan()
{
	WorkDays = new Dictionary<DayOfWeek, WorkDaySpan>()
	{
		{DayOfWeek.Monday, new WorkDaySpan() {TimeSpans = wts}}
		,
		{DayOfWeek.Tuesday, new WorkDaySpan() {TimeSpans = wts}}
		,
		{DayOfWeek.Wednesday, new WorkDaySpan() {TimeSpans = wts}}
		,
		{DayOfWeek.Thursday, new WorkDaySpan() {TimeSpans = wts}}
		,
		{DayOfWeek.Friday, new WorkDaySpan() {TimeSpans = wts}}
	}
};

//this is the configuration for holidays: 
//in Italy we have this list of Holidays plus 1 day different on each province,
//for mine is 1 Dec (see last element of the List<AHolyDay>).
var italiansHoliDays = new List<AHolyDay>()
{
	new EasterMonday(),new HoliDay(1, 1),new HoliDay(6, 1),
	new HoliDay(25, 4),new HoliDay(1, 5),new HoliDay(2, 6),
	new HoliDay(15, 8),new HoliDay(1, 11),new HoliDay(8, 12),
	new HoliDay(25, 12),new HoliDay(26, 12)
	, new HoliDay(1, 12)
};

//instantiate with configuration
var utility = new WorkingDaysAndTimeUtility(week, italiansHoliDays);

//lets-go: add 3 working-days to Jun 1, 2015
var result = utility.AddWorkingDays(new DateTime(2015,6,1), 3);
//result is Jun 5, 2015 (see holidays list) 

GetWorkingDaysBetweenTwoDateTimes(DateTime start, DateTime end, bool includeStartAndEnd = true)

var start = new DateTime(2015, 12, 31, 9, 0, 0);
var end = new DateTime(2016, 1, 7, 9, 0, 0);

//omitted configurations and holidays...
var utility = new WorkingDaysAndTimeUtility(weekConf, GetItalianHolidays());

//r is a workdays List<DateTime> between Dec 31 and Jan 7.
var r = utility.GetWorkingDaysBetweenTwoDateTimes(start, end);

Testing if given date is Working-Datetime

[Fact]
public void Get_IfWorkingDay_OnTuesday_OnSimpleWeek_ReturnTrue()
{

    //omitted configurations and holidays...
    var tuesday = new DateTime(2018, 11, 6, 11,22,33);
    var prev0 = tuesday.AddMinutes(-1);
    var next0 = tuesday.AddMinutes(1);

    var weekConf = GetSimpleWeek();
    var utility  = new WorkingDaysAndTimeUtility(weekConf, new List<HoliDay>());

    var r = utility.IfWorkingMoment(tuesday, out DateTime next, out DateTime previous);

    Assert.True(r);
    Assert.Equal(prev0, previous);
    Assert.Equal(next0, next);

}

Code Configuration Examples

Use of WorkingDaysConfig

//note thats w is WeekDaySpan and l is List<AHolyDay>
//cfg is Json serializable
var cfg = new WorkingDaysConfig(w, l);

Map-Config Style

var cfg = new WorkingDaysConfig()
                      .Week(new WeekDaySpan().Day(DayOfWeek.Monday,
                                                  new WorkDaySpan()
                                                      .Time(new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0))
                                                      .Time(new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0)))
                                             .Day(DayOfWeek.Tuesday,
                                                  new WorkDaySpan()
                                                      .Time(new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0))
                                                      .Time(new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0)))
                           )
                      .Holiday(new AHolyDay(15, 8))
                      .Holiday(2, 6)
                      .Holiday(new EasterMonday());

var cfg2 = new WorkingDaysConfig().Week(WeekDaySpan.CreateSymmetricalConfig(new WorkDaySpan()
																			.Time(new TimeSpan(9, 0, 0),
																					new TimeSpan(13, 0, 0))
																			.Time(new TimeSpan(14, 0, 0),
																					new TimeSpan(18, 0, 0)),
																			new DayOfWeek[]
																			{
																				DayOfWeek.Monday,
																				DayOfWeek.Tuesday,
																				DayOfWeek.Wednesday,
																				DayOfWeek.Thursday,
																				DayOfWeek.Friday
																			}));


 
 */
