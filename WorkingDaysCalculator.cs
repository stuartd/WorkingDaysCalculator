using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Working_Days_Calculator {
    public static class WorkingDaysCalculator {

        public static DateTime GetNextWorkingDay(DateTime startDate, IList<DateTime> extraHolidays = null) {
            // If the date itself isn't a working day, walk FORWARD until we find one.
            // (The start date and end dates of leave should be working days)
            var bankHolidays = GetBankHolidaysInternal(startDate.Year, extraHolidays);

            while (!IsWorkingDay(startDate, bankHolidays)) {
                startDate = startDate.AddDays(1);
            }

            return startDate;
        }

        public static DateTime GetPreviousWorkingDay(DateTime startDate, IList<DateTime> extraHolidays = null) {
            // If the date itself isn't a working day, walk back until we find one.
            // (The start date and end dates of leave should be working days)
            var bankHolidays = GetBankHolidaysInternal(startDate.Year, extraHolidays);

            while (!IsWorkingDay(startDate, bankHolidays)) {
                startDate = startDate.AddDays(-1);
            }

            return startDate;
        }

        public static DateTime GetLastWorkingDay(DateTime startDate, int days, IList<DateTime> extraHolidays = null) {
            if (days < 1) {
                return startDate;
            }

            var bankHolidays = GetBankHolidaysInternal(startDate.Year, extraHolidays);

            startDate = GetNextWorkingDay(startDate, bankHolidays);

            // Walk a day at a time from the start. If the day is a weekend or bank holiday,
            // then don't subtract a day. When there are no days left, that's the last working date.

            int i = 1;
            var target = startDate;

            // The start date is a working day, so drop the number of days by one:
            days--;
            // Then keep walking forward: each working day decrements days.
            while (days > 0) {
                target = startDate.AddDays(i);
                if (IsWorkingDay(target, bankHolidays)) {
                    days--;
                }

                i++;
            }

            return target; // which is now the last working day of the leave.
        }

        private static bool IsWorkingDay(DateTime day, IEnumerable<DateTime> bankHolidays) {
            return day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday && !bankHolidays.Contains(day);
        }

        private static DateTime[] GetBankHolidaysInternal(int year, IList<DateTime> extraHolidays = null) {
            // Working day queries can legitimately cross to the next year - it's up to the caller to make sure
            // any extra holidays in *next* year are passed as well.
            return GetBankHolidays(year).Union(GetBankHolidays(year + 1).Union(extraHolidays ?? new DateTime[0])).ToArray();
        }

        // Used for working out contract working days, doesn't do error checking.
        public static DateTime[] GetWorkingDaysExcludingBankHolidays(int year, int month) {
            var start = new DateTime(year, month, 1);
            var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            List<DateTime> workingDays = new List<DateTime>();

            var workingDate = start.Date;
            var bankHolidays = GetBankHolidays(year);

            while (workingDate <= end) {
                if (workingDate.DayOfWeek != DayOfWeek.Saturday
                     && workingDate.DayOfWeek != DayOfWeek.Sunday) {
                    if (bankHolidays.Contains(workingDate)) {
                        workingDays.Add(workingDate.Date.AddSeconds(1));
                    }
                    else {
                        workingDays.Add(workingDate.Date);
                    }
                }

                workingDate = workingDate.AddDays(1);
            }

            return workingDays.ToArray();
        }

        public static DateTime[] GetWorkingDays(DateTime start, DateTime end, List<DateTime> extraBankHolidays = null) {
            if (end < start) {
                throw new ArgumentOutOfRangeException(nameof(end), "End date may not be before start date");
            }

            if (start == DateTime.MinValue) {
                throw new ArgumentOutOfRangeException(nameof(start), "Start date must have a value");
            }

            if (end == DateTime.MinValue) {
                throw new ArgumentOutOfRangeException(nameof(start), "End date must have a value");
            }

            if (end.Year > DateTime.Today.Year + 10) {
                throw new ArgumentOutOfRangeException(nameof(start), "End date can't be that far away.");
            }

            if (extraBankHolidays == null) {
                extraBankHolidays = new List<DateTime>();
            }

            List<DateTime> workingDays = new List<DateTime>();

            var workingDate = start.Date;
            end = end.Date.AddDays(1);

            int currentYear = workingDate.Year;
            List<DateTime> holidays = GetBankHolidays(currentYear).Union(extraBankHolidays).ToList();

            while (workingDate < end) {
                if (workingDate.DayOfWeek != DayOfWeek.Saturday
                     && workingDate.DayOfWeek != DayOfWeek.Sunday
                     && holidays.Contains(workingDate.Date) == false) {
                    workingDays.Add(workingDate.Date);
                }

                workingDate = workingDate.AddDays(1);
                if (workingDate.Year != currentYear) {
                    currentYear = workingDate.Year;
                    holidays = GetBankHolidays(currentYear).Union(extraBankHolidays).ToList();
                }
            }

            return workingDays.ToArray();
        }

        private static readonly Dictionary<int, IList<DateTime>> calculatedHolidays = new Dictionary<int, IList<DateTime>>();

        public static IList<DateTime> GetBankHolidays(int year) {
            return GetBankHolidaysForYear(year);
        }

        private static IList<DateTime> GetBankHolidaysForYear(int year) {
            // ReSharper disable InconsistentlySynchronizedField - only locked when adding
            if (calculatedHolidays.ContainsKey(year)) {
                return calculatedHolidays[year];
            }

            var holidays = new List<DateTime> {
                    NewYearsDay(year),
                    GoodFriday(year),
                    EasterMonday(year),
                    MayDayBankHoliday(year),
                    SpringBankHoliday(year),
                    SummerBankHoliday(year),
                    ChristmasDay(year),
                    BoxingDay(year)
            };

            calculatedHolidays.Add(year, holidays);

            return holidays;
        }

        private static DateTime ChristmasDay(int year) {
            var start = new DateTime(year, 12, 25);

            if (start.DayOfWeek == DayOfWeek.Saturday) {
                return start.AddDays(2);
            }

            if (start.DayOfWeek == DayOfWeek.Sunday) {
                return start.AddDays(1);
            }

            return start;
        }

        private static DateTime BoxingDay(int year) {
            var start = new DateTime(year, 12, 26);

            if (start.DayOfWeek == DayOfWeek.Saturday) {
                return start.AddDays(2);
            }

            if (start.DayOfWeek == DayOfWeek.Sunday) {
                return start.AddDays(2); // The Monday will be the substitute day for Christmas Day
            }

            if (start.DayOfWeek == DayOfWeek.Monday) {
                return start.AddDays(1); // The Monday will still be the substitute day for Christmas Day
            }

            return start;
        }

        private static DateTime NewYearsDay(int year) {
            var start = new DateTime(year, 1, 1);

            if (start.DayOfWeek == DayOfWeek.Saturday) {
                return start.AddDays(2);
            }

            if (start.DayOfWeek == DayOfWeek.Sunday) {
                return start.AddDays(1);
            }

            return start;
        }

        private static DateTime GoodFriday(int year) {
            return EasterSunday(year).AddDays(-2);
        }

        private static DateTime EasterMonday(int year) {
            return EasterSunday(year).AddDays(1);
        }

        private static DateTime SpringBankHoliday(int year) {
            // Last Monday in May
            return DateUtilities.LastDayOfTheWeekInMonth(year, 5, DayOfWeek.Monday);
        }

        private static DateTime SummerBankHoliday(int year) {
            // last Monday in August
            return DateUtilities.LastDayOfTheWeekInMonth(year, 8, DayOfWeek.Monday);
        }

        private static DateTime MayDayBankHoliday(int year) {
            // First Monday in May
            return DateUtilities.FirstDayOfTheWeekInMonth(year, 5, DayOfWeek.Monday);
        }

        private static DateTime EasterSunday(int year) {
            // Tidied up a little from code at
            // http://bloggingabout.net/blogs/jschreuder/archive/2005/06/24/7019.aspx

            var g = year % 19;
            var c = year / 100;
            var h = (c - c / 4 - (8 * c + 13) / 25 + 19 * g + 15) % 30;
            var i = h - h / 28 * (1 - h / 28 * (29 / (h + 1)) * ((21 - g) / 11));
            var day = i - (year + year / 4 + i + 2 - c + c / 4) % 7 + 28;

            var month = 3;
            if (day > 31) {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }
    }

    public static class DateUtilities {

        public static DateTime LastDayOfTheWeekInMonth(int year, int month, DayOfWeek day) {
            var start = FirstDayOfTheWeekInMonth(year, month, day);
            var lastDayInMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            while (start.AddDays(6) < lastDayInMonth) {
                start = start.AddDays(7);
            }

            return start;
        }

        public static DateTime FirstDayOfTheWeekInMonth(int year, int month, DayOfWeek day) {
            var firstOfMonth = new DateTime(year, month, 1);
            var firstDay = firstOfMonth.AddDays((int)day - (int)firstOfMonth.DayOfWeek);

            if (firstDay < firstOfMonth) {
                firstDay = firstDay.AddDays(7);
            }

            return firstDay;
        }
    }
}
