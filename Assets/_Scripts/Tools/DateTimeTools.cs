using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
namespace Hedge
{
    namespace Tools
    {
        static public class DateTimeTools
        {
            static public double TicksPerFloat = DateTime.MaxValue.Ticks / float.MaxValue;
            static public double FloatsPerTick = float.MaxValue / DateTime.MaxValue.Ticks;

            static public DateTime TimestampToDate(double timeStamp)
            {
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(timeStamp);
                return dt;
            }
            static public double DateToTimestamp(DateTime date)
            {
                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                TimeSpan diff = date - origin;
                return Math.Floor(diff.TotalSeconds);
            }

            static public void RoundTimeStampTo(this double timestamp, TimeFrame tFrame, out double periodBegin, out double periodEnd, TimeSpan offset)
            {
                double devider;
                periodBegin = 0;
                periodEnd = 0;
                switch (tFrame.period)
                {
                    case Period.Second: { } break;
                    case Period.Minute:
                        {
                            devider = tFrame.count * 60;
                            periodBegin = timestamp - (timestamp % devider);
                            periodEnd = periodBegin + devider;
                        }
                        break;

                    case Period.Hour:
                        {
                            devider = tFrame.count * 3600;
                            periodBegin = timestamp - (timestamp % devider);
                            periodEnd = periodBegin + devider;
                        }
                        break;

                    case Period.Day:
                        {
                            devider = tFrame.count * 86400;
                            periodBegin = timestamp - (timestamp % devider);
                            periodEnd = periodBegin + devider;
                        }
                        break;

                    case Period.Week:
                        {
                            devider = tFrame.count * 604800;
                            periodBegin = timestamp - (timestamp % devider);
                            periodEnd = periodBegin + devider;

                        }
                        break;

                    case Period.Month:
                        {
                            DateTimeOffset dt = new DateTimeOffset(TimestampToDate(timestamp), offset);
                            dt = new DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, offset);
                            periodBegin = DateToTimestamp(dt.DateTime);
                            periodEnd = DateToTimestamp(dt.DateTime.AddMonths(1));
                        }
                        break;
                    case Period.Year:
                        {
                            DateTimeOffset dt = new DateTimeOffset(TimestampToDate(timestamp), offset);
                            dt = new DateTimeOffset(dt.Year, 1, 1, 0, 0, 0, offset);
                            periodBegin = DateToTimestamp(dt.DateTime);
                            periodEnd = DateToTimestamp(dt.DateTime.AddYears(1));

                        }
                        break;
                    default:
                        {
                            Debug.Log("Задано не верное значение tFrame.period");

                        }
                        break;

                }
            }

            static public int CountFramesInPeriod(TimeFrame tframe, double periodBeginTimestamp, double periodEndTimestamp)
            {
                int count;
                double frames_count;
                double periodLength = periodEndTimestamp - periodBeginTimestamp;

                switch (tframe.period)
                {
                    case Period.Minute:
                        {
                            frames_count = (periodLength / (tframe.count * 60));
                        }
                        break;
                    case Period.Hour:
                        {
                            frames_count = (periodLength / (tframe.count * 3600));
                        }
                        break;
                    case Period.Day:
                        {
                            frames_count = (periodLength / (tframe.count * 86400));
                        }
                        break;
                    case Period.Week:
                        {
                            frames_count = (periodLength / (tframe.count * 604800));
                        }
                        break;
                    case Period.Month:
                        {
                            DateTimeOffset dtBegin = new DateTimeOffset(TimestampToDate(periodBeginTimestamp));
                            DateTimeOffset dtEnd = new DateTimeOffset(TimestampToDate(periodEndTimestamp));
                            frames_count = (double)(dtEnd.Month - dtBegin.Month + 12 * (dtEnd.Year - dtBegin.Year)) / tframe.count;
                        }
                        break;
                    case Period.Year:
                        {
                            DateTimeOffset dtBegin = new DateTimeOffset(TimestampToDate(periodBeginTimestamp));
                            DateTimeOffset dtEnd = new DateTimeOffset(TimestampToDate(periodEndTimestamp));
                            frames_count = (double)(dtEnd.Year - dtBegin.Year) / tframe.count;
                        }
                        break;

                    default:
                        {
                            Debug.Log("Указан не тип периода, который не реализован");
                            return -1;
                        }
                }

                count = (int)frames_count;
                if (frames_count != count) count++;
                return count;
            }
            static public int CountFramesInPeriod(TimeFrame tframe, DateTime firstDate, DateTime secondDate, TimeSpan offset)
            {
                int count;
                double frames_amount;
                long periodLength = secondDate.Ticks - firstDate.Ticks;

                switch (tframe.period)
                {
                    case Period.Minute:
                        {
                            frames_amount = (double)periodLength / new TimeSpan(0, tframe.count, 0).Ticks;
                        }
                        break;
                    case Period.Hour:
                        {
                            frames_amount = (double)periodLength / new TimeSpan(tframe.count, 0, 0).Ticks;

                        }
                        break;
                    case Period.Day:
                        {
                            frames_amount = (double)periodLength / new TimeSpan(tframe.count, 0, 0, 0).Ticks;

                        }
                        break;
                    case Period.Week:
                        {
                            frames_amount = (double)periodLength / new TimeSpan(7 * tframe.count, 0, 0, 0).Ticks;


                        }
                        break;
                    case Period.Month:
                        {
                            DateTimeOffset dtBegin = new DateTimeOffset(firstDate, offset);
                            DateTimeOffset dtEnd = new DateTimeOffset(secondDate, offset);
                            frames_amount = (dtEnd.Month - dtBegin.Month + (double)12 * (dtEnd.Year - dtBegin.Year)) / tframe.count;

                        }
                        break;
                    case Period.Year:
                        {
                            DateTimeOffset dtBegin = new DateTimeOffset(firstDate, offset);
                            DateTimeOffset dtEnd = new DateTimeOffset(secondDate, offset);
                            frames_amount = (double)(dtEnd.Year - dtBegin.Year) / tframe.count;

                        }
                        break;

                    default:
                        {
                            Debug.Log("Указан тип периода, который не реализован");
                            return -1;
                        }
                        
                }

                frames_amount = Math.Abs(frames_amount);
                count = (int)frames_amount;
                if (frames_amount != count) count++;
                return count;
            }

            //Округление до начала текущего периода для разных временных рамок
            public static DateTime FloorToMinutes(this DateTime dt, int minutes_amount=1)
            {
                TimeSpan interval = new TimeSpan(0, 0, minutes_amount, 0, 0);

                long roundedTicks = (dt.Ticks / interval.Ticks) * interval.Ticks;

                return new DateTime(roundedTicks);
            }
            public static DateTime FloorToHours(this DateTime dt, int hours_amount =1)
            {
                TimeSpan interval = new TimeSpan(0, hours_amount, 0, 0, 0);

                long roundedTicks = (dt.Ticks / interval.Ticks) * interval.Ticks;

                return new DateTime(roundedTicks);
            }
            public static DateTime FloorToDays(this DateTime dt, int days_amount =1)
            {
                TimeSpan interval = new TimeSpan(days_amount, 0, 0, 0, 0);

                long roundedTicks = (dt.Ticks / interval.Ticks) * interval.Ticks;

                return new DateTime(roundedTicks);
            }
            public static DateTime FloorToWeeks(this DateTime dt, int weeks_amount)
            {
                TimeSpan interval = new TimeSpan(7 * weeks_amount, 0, 0, 0, 0);

                long roundedTicks = (dt.Ticks / interval.Ticks) * interval.Ticks;
                dt = new DateTime(roundedTicks);
                return dt.AddDays(-((int)dt.DayOfWeek - 1));

            }
            public static DateTime FloorToMonths(this DateTime dt, int monthes_amount =1)
            {
                return new DateTime().AddMonths(monthes_amount * (int)((double)(dt.Year-1) * 12 + dt.Month-1) / monthes_amount);
            }
            public static DateTime FloorToYears(this DateTime dt, int years_amount=1)
            {
                return new DateTime(years_amount * (dt.Year / years_amount), 1, 1);
            }

            public static DateTime FloorToTimeFrame(this DateTime dt, TimeFrame timeFrame)
            {
                switch (timeFrame.period)
                {
                    case Period.Minute: { return dt.FloorToMinutes(timeFrame.count); } 
                    case Period.Hour: { return dt.FloorToHours(timeFrame.count); } 
                    case Period.Day: { return dt.FloorToDays(timeFrame.count); } 
                    case Period.Week: { return dt.FloorToWeeks(timeFrame.count); } 
                    case Period.Month: { return dt.FloorToMonths(timeFrame.count); } 
                    case Period.Year: { return dt.FloorToYears(timeFrame.count); } 
                    default: { Debug.LogError("Период не задан"); } break;

                }
                return new DateTime();
            }

            //Округление вперёд - до начала следующего периода для разных временных рамок
            static public DateTime UpToMinutes(this DateTime dt, int minutes_amount)
            {
                TimeSpan interval = new TimeSpan(0, 0, minutes_amount, 0, 0);

                long roundedTicks = (dt.Ticks / interval.Ticks) * interval.Ticks + interval.Ticks;

                return new DateTime(roundedTicks);
            }
            static public DateTime UpToHours(this DateTime dt, int hours_amount)
            {
                TimeSpan interval = new TimeSpan(0, hours_amount, 0, 0, 0);

                long roundedTicks = (dt.Ticks / interval.Ticks) * interval.Ticks + interval.Ticks;

                return new DateTime(roundedTicks);
            }
            static public DateTime UpToDays(this DateTime dt, int days_amount=1)
            {
                TimeSpan interval = new TimeSpan(days_amount, 0, 0, 0, 0);

                long roundedTicks = (dt.Ticks / interval.Ticks) * interval.Ticks + interval.Ticks;

                return new DateTime(roundedTicks);
            }

            static public DateTime UpToWeeks(this DateTime dt, int weeks_amount=1)
            {
                TimeSpan interval = new TimeSpan(7 * weeks_amount, 0, 0, 0, 0);

                long roundedTicks = (dt.Ticks / interval.Ticks) * interval.Ticks + interval.Ticks;
                dt = new DateTime(roundedTicks);
                return dt.AddDays(-((int)dt.DayOfWeek - 1));

            }
            static public DateTime UpToMonths(this DateTime dt, int monthes_amount=1)
            {
                return new DateTime().AddMonths(monthes_amount * ((int)((double)(dt.Year-1) * 12 + dt.Month - 1) / monthes_amount + 1));
            }
            static public DateTime UpToYears(this DateTime dt, int years_amount=1)
            {
                return new DateTime(years_amount * (dt.Year / years_amount) + years_amount, 1, 1);
            }
            static public DateTime UpToNextFrame(this DateTime dt, TimeFrame timeFrame)
            {
                switch (timeFrame.period)
                {
                    case Period.Minute: { return dt.UpToMinutes(timeFrame.count); } 
                    case Period.Hour: { return dt.UpToHours(timeFrame.count); } 
                    case Period.Day: { return dt.UpToDays(timeFrame.count); } 
                    case Period.Week: { return dt.UpToWeeks(timeFrame.count); } 
                    case Period.Month: { return dt.UpToMonths(timeFrame.count); } 
                    case Period.Year: { return dt.UpToYears(timeFrame.count); }
                    default: { Debug.LogError("Период не задан"); } break;

                }
                return new DateTime();
            }

            static public float GetPositionByTimeFrame(this DateTime dt)
            {
                return (float)(dt.Ticks / TicksPerFloat);
            }
            static public DateTime GetPositionByTimeFrame(this float xCord)
            {
                return new DateTime((long)(xCord / FloatsPerTick));
            }

            static public string ChartStringFormat(this DateTime dt)
            {
                string output;
                if (dt.Minute == 0)
                {
                    if (dt.Hour == 0)
                    {
                        if (dt.Day == 1)
                        {
                            if (dt.Month == 1)
                            {
                                output = String.Format("{0:yyyy}", dt);
                            }
                            else
                            {
                                output = String.Format("{0:MMM}", dt).ToUpper();
                            }
                        }
                        else
                        {
                            output = String.Format("{0:dd}", dt).ToUpper();
                        }
                    }
                    else
                    {
                        output = String.Format("{0:HH:mm}", dt);
                    }

                }
                else
                {
                    output = String.Format("{0:HH:mm}", dt);
                }

                return output;
            }

            static public int[] possibleMonthStep = new int[] { 1, 2, 3, 4, 6 };
            static public int[] possibleHourStep = new int[] { 1, 2, 3, 4, 6, 8, 12 };
            static public int[] possibleDayStep = new int[] { 1, 2, 3, 4, 5, 6, 7, 9, 14 };
            static public int[] possibleMinuteStep = new int[] { 1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 30 };
            //DateTime dt0, dt1;
            static public IEnumerable<DateTime> DividePeriodByKeyPoints(DateTime first, DateTime second, int divisorsMaxAmount) 
            {

                List<DateTime> keyPoints = new List<DateTime>();


                divisorsMaxAmount = divisorsMaxAmount - 1;

                TimeSpan periodLenth = second - first;
                double yearsStep = periodLenth.TotalDays / 365.25;
                double monthsStep = periodLenth.TotalDays / 30.4375;
                double daysStep = periodLenth.TotalDays;
                double hourStep = periodLenth.TotalHours;
                double minuteStep = periodLenth.TotalMinutes;


                yearsStep /= divisorsMaxAmount;
                monthsStep /= divisorsMaxAmount;
                daysStep /= (divisorsMaxAmount - (int)daysStep/28);
                hourStep /= divisorsMaxAmount;
                minuteStep /= divisorsMaxAmount;
                
                int step;

                //Debug.Log("Годовой шаг:" + yearsStep);
                //Debug.Log("Месячный шаг:" + monthsStep);
                //Debug.Log("Дневной шаг:" + daysStep);
                //Debug.Log("Часовой шаг:" + hourStep);
                //Debug.Log("Минутный шаг:" + minuteStep);
                TimeFrame frame = new TimeFrame();

                //Определение корректного шага для отображения
                if (yearsStep > 0.5)
                {
                    frame.period = Period.Year;

                    if (yearsStep != (int)yearsStep) yearsStep++;
                    step = (int)yearsStep;

                }
                else if (monthsStep > 0.5)
                {
                    frame.period = Period.Month;

                    step = possibleMonthStep.Where(possibleStep => monthsStep <= possibleStep).DefaultIfEmpty(possibleMonthStep.Max()).Min();
                }
                else if (daysStep > 0.5)
                {
                    frame.period = Period.Day;

                    step = possibleDayStep.Where(possibleStep => daysStep <= possibleStep).DefaultIfEmpty(possibleDayStep.Max()).Min();
                }
                else if (hourStep > 0.5)
                {
                    frame.period = Period.Hour;
                    step = possibleHourStep.Where(possibleStep => hourStep <= possibleStep).DefaultIfEmpty(possibleHourStep.Max()).Min();
                }
                else if (minuteStep > 0.5)
                {
                    frame.period = Period.Minute;
                    step = possibleMinuteStep.Where(possibleStep => minuteStep <= possibleStep).DefaultIfEmpty(possibleMinuteStep.Max()).Min();
                }
                else
                {
                    Debug.Log("Слишком маленький промежуток");
                    return null;
                }
                frame.count = step;

                DateTime current_time;

                if (frame.period != Period.Day)
                {
                    current_time = first.UpToNextFrame(frame);
                    keyPoints.Add(current_time);
                    while (current_time <= second)
                    {
                        current_time += frame;
                        keyPoints.Add(current_time);
                    }
                }
                else
                {
                    DateTime tmp;

                    current_time = first.FloorToDays().AddDays(frame.count* ((first.Day-1) / frame.count + 1) - first.Day+1);
                    tmp = first.UpToMonths();
                    double tmp1 = (tmp - current_time).TotalDays / frame.count;                 
                    if (current_time.Month != first.Month || tmp1 <0.5)
                    {
                        current_time = tmp;
                    }
                    keyPoints.Add(current_time);
                    current_time += frame;

                    while (current_time < second)
                    {
                        
                        tmp1 = (tmp-current_time).TotalDays / frame.count;
                        if (tmp1 < 0.5 || current_time.Month !=keyPoints.Last().Month)
                        {
                            current_time = tmp;
                        }
                        keyPoints.Add(current_time);
                        tmp = current_time.UpToMonths();
                        current_time += frame;

                    }
                }

                return keyPoints;
            }

            static double tmp = 0;
            static public IEnumerable<DateTime> DividePeriodByKeyPointsAlternative(DateTime first, DateTime second, int divisorsMaxAmount, TimeFrame chartTimeFrame)
            {
                List<DateTime> keyPoints = new List<DateTime>();

                TimeSpan dateDiff = second - first;
                double dateDifference;
                long dateDiffInTicks = dateDiff.Ticks;
                long stepTicks = dateDiffInTicks / divisorsMaxAmount;
                TimeSpan stepTS = new TimeSpan(stepTicks);
                int step;
                TimeFrame timeFrame;
                Period period;
                Debug.Log("Делителей: " + divisorsMaxAmount + "; Разница дат: " + dateDiff.ToString());
                if (stepTS.TotalDays > 365)
                {
                    //1+ необходимо для увеличения размера шага, так как int округлит деление в меньшую сторону и делителей не хватит
                    dateDifference = second.Year - first.Year;
                    period = Period.Year;
                }
                else if (stepTS.TotalDays > 31)
                {
                    dateDifference = (second.Year - first.Year) * 12 + second.Month - first.Month;
                    period = Period.Month;
                }
                else if (stepTS.TotalDays > 1)
                {
                    dateDifference = (second - first).TotalDays;
                    period = Period.Day;
                }
                else if (stepTS.TotalHours > 1)
                {
                    dateDifference = (second - first).TotalHours;
                    period = Period.Hour;
                }

                else if (stepTS.TotalMinutes > 1)
                {
                    dateDifference = (second - first).TotalMinutes;
                    period = Period.Minute;
                }
                else
                {
                    Debug.Log("Слишком маленький промежуток");
                    return null;
                }

                if (Math.Abs(tmp / dateDifference - 1) >= 0.05)
                {
                    tmp = dateDifference;
                }
                //Debug.Log(dateDifference + " " + tmp);
                step = (int)(1 + tmp / (divisorsMaxAmount - 2));
                timeFrame = new TimeFrame(period, step);
                DateTime next_date;
                DateTime current_date;
                DateTime floor_date;

                //Debug.Log(dt0.ToShortTimeString() + " " + dt0.FloorToTimeFrame(timeFrame).ToShortTimeString() + " " + dt0.FloorToTimeFrame(timeFrame).ToShortTimeString());

                current_date = first.UpToNextFrame(timeFrame);

                next_date = current_date;
                if (current_date.Year > first.Year)
                {
                    next_date = current_date.FloorToYears();
                }
                else if (current_date.Month != first.Month)
                {
                    next_date = current_date.FloorToMonths();
                }
                else if (current_date.Day != first.Day)
                {
                    next_date = current_date.FloorToDays();

                }
                else if (current_date.Hour != first.Hour)
                {
                    if (chartTimeFrame.period == Period.Hour)
                    { next_date = next_date.FloorToTimeFrame(chartTimeFrame); }
                    else
                    { next_date = next_date.FloorToHours(); }
                }

                if (next_date > first)
                {
                    keyPoints.Add(next_date);
                    Debug.Log(1);
                }
                else
                {
                    keyPoints.Add(current_date.FloorToTimeFrame(chartTimeFrame));
                    Debug.Log(2);

                }

                while (current_date < second)
                {
                    next_date = current_date + timeFrame;

                    if (next_date.Year > current_date.Year)
                    {
                        floor_date = next_date.FloorToYears();
                    }
                    else if (next_date.Month != current_date.Month)
                    {

                        floor_date = next_date.FloorToMonths();
                    }
                    else if (next_date.Day != current_date.Day)
                    {
                        if (chartTimeFrame.period == Period.Day)
                        {
                            floor_date = next_date.FloorToTimeFrame(chartTimeFrame);
                        }
                        else
                        {
                            floor_date = next_date.FloorToDays();
                        }

                    }
                    else if (next_date.Hour != current_date.Hour)
                    {
                        //int count = chartTimeFrame.period == Period.Hour ? chartTimeFrame.count : 1;
                        if (chartTimeFrame.period == Period.Hour)
                        { floor_date = next_date.FloorToTimeFrame(chartTimeFrame); }
                        else
                        { floor_date = next_date.FloorToHours(); }
                    }
                    else
                    {
                        floor_date = next_date;
                        keyPoints.Add(current_date.FloorToTimeFrame(chartTimeFrame));
                    }

                    if (2 * floor_date.Ticks - current_date.Ticks > next_date.Ticks)
                    {

                        keyPoints.Add(floor_date);
                    }
                    else
                    {
                        if (keyPoints.Count != 0)
                            keyPoints[keyPoints.Count - 1] = floor_date;
                    }

                    current_date = next_date;
                }
                return keyPoints;
            }
        }

        public enum Period
        {
            Tick,
            Second,
            Minute,
            Hour,
            Day,
            Week,
            Month,
            Year
        }
        public struct TimeFrame
        {
            public int count;
            public Period period;

            public TimeFrame(Period period, int count = 1)
            {
                if (count <= 0) count = 1;
                this.count = count;
                this.period = period;
            }
            public static TimeFrame operator *(TimeFrame tFrame, int multiplicator)
            {

                return multiplicator * tFrame;
            }

            public static TimeFrame operator *(int multiplicator, TimeFrame tFrame)
            {
                tFrame.count *= multiplicator;
                return tFrame;
            }
            public static TimeFrame operator *(TimeFrame tFrame, float multiplicator)
            {

                return multiplicator * tFrame;
            }

            public static TimeFrame operator *(float multiplicator, TimeFrame tFrame)
            {
                tFrame.count = (int)(tFrame.count *multiplicator);
                return tFrame;
            }
            public static DateTime operator +(DateTime dateTime, TimeFrame tFrame)
            {
                switch (tFrame.period)
                {
                    case Period.Minute: { return dateTime.AddMinutes(tFrame.count); } 
                    case Period.Hour: { return dateTime.AddHours(tFrame.count); } 
                    case Period.Day: { return dateTime.AddDays(tFrame.count); } 
                    case Period.Week: { return dateTime.AddDays(7 * tFrame.count); } 
                    case Period.Month: { return dateTime.AddMonths(tFrame.count); } 
                    case Period.Year: { return dateTime.AddYears(tFrame.count); }
                    default:
                        {
                            throw new System.ArgumentOutOfRangeException("Сначала реализуй алгоритм сложения");
                        }
                        
                }


            }

            public static DateTime operator +(TimeFrame tFrame, DateTime dateTime)
            {
                return dateTime + tFrame;
            }
            public static DateTime operator -(DateTime dateTime, TimeFrame tFrame)
            {
                return dateTime + (-1)*tFrame;
            }
        }
    }
}