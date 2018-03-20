using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
                        break;
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
                return new DateTime(0, monthes_amount * (int)((double)dt.Year * 12 + dt.Month) / monthes_amount, 1);
            }
            public static DateTime FloorToYears(this DateTime dt, int years_amount)
            {
                return new DateTime(years_amount * (dt.Year / years_amount), 0, 1);
            }

            public static DateTime FloorToTimeFrame(this DateTime dt, TimeFrame timeFrame)
            {
                switch (timeFrame.period)
                {
                    case Period.Minute: { return dt.FloorToMinutes(timeFrame.count); } break;
                    case Period.Hour: { return dt.FloorToHours(timeFrame.count); } break;
                    case Period.Day: { return dt.FloorToDays(timeFrame.count); } break;
                    case Period.Week: { return dt.FloorToWeeks(timeFrame.count); } break;
                    case Period.Month: { return dt.FloorToMonths(timeFrame.count); } break;
                    case Period.Year: { return dt.FloorToYears(timeFrame.count); } break;
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
            static public DateTime UpToDays(this DateTime dt, int days_amount)
            {
                TimeSpan interval = new TimeSpan(days_amount, 0, 0, 0, 0);

                long roundedTicks = (dt.Ticks / interval.Ticks) * interval.Ticks + interval.Ticks;

                return new DateTime(roundedTicks);
            }

            static public DateTime UpToWeeks(this DateTime dt, int weeks_amount)
            {
                TimeSpan interval = new TimeSpan(7 * weeks_amount, 0, 0, 0, 0);

                long roundedTicks = (dt.Ticks / interval.Ticks) * interval.Ticks + interval.Ticks;
                dt = new DateTime(roundedTicks);
                return dt.AddDays(-((int)dt.DayOfWeek - 1));

            }
            static public DateTime UpToMonths(this DateTime dt, int monthes_amount)
            {
                return new DateTime().AddMonths(monthes_amount * (((dt.Year-1) * 12 + dt.Month-1) / monthes_amount) + 1);
            }
            static public DateTime UpToYears(this DateTime dt, int years_amount)
            {
                return new DateTime(years_amount * (dt.Year / years_amount) + years_amount, 0, 1);
            }
            static public DateTime UpToNextFrame(this DateTime dt, TimeFrame timeFrame)
            {
                switch (timeFrame.period)
                {
                    case Period.Minute: { return dt.UpToMinutes(timeFrame.count); } break;
                    case Period.Hour: { return dt.UpToHours(timeFrame.count); } break;
                    case Period.Day: { return dt.UpToDays(timeFrame.count); } break;
                    case Period.Week: { return dt.UpToWeeks(timeFrame.count); } break;
                    case Period.Month: { return dt.UpToMonths(timeFrame.count); } break;
                    case Period.Year: { return dt.UpToYears(timeFrame.count); } break;
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
            public static DateTime operator +(DateTime dateTime, TimeFrame tFrame)
            {
                switch (tFrame.period)
                {
                    case Period.Minute: { return dateTime.AddMinutes(tFrame.count); } break;
                    case Period.Hour: { return dateTime.AddHours(tFrame.count); } break;
                    case Period.Day: { return dateTime.AddDays(tFrame.count); } break;
                    case Period.Week: { return dateTime.AddDays(7 * tFrame.count); } break;
                    case Period.Month: { return dateTime.AddMonths(tFrame.count); } break;
                    case Period.Year: { return dateTime.AddYears(tFrame.count); } break;
                    default:
                        {
                            throw new System.ArgumentOutOfRangeException("Снала реализуй алгоритм сложения");
                        }
                        break;
                }


            }

            public static DateTime operator +(TimeFrame tFrame, DateTime dateTime)
            {
                return dateTime + tFrame;
            }
        }
    }
}