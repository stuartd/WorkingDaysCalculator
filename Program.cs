using System;

namespace Working_Days_Calculator {
    class Program {
        static void Main(string[] args) {
			int month = 1;
			int year = 2019;
			bool writeDays = true;
			bool writeDaysIncludingBankHolidays = false;
            const decimal dailyRate = 344.83M;
			const decimal adjustment = -0.5M;
			
			var start = new DateTime(year, month, 1);
			var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));

			Console.WriteLine($"Working days in {month}/{year}");
			var days = WorkingDaysCalculator.GetWorkingDays(start, end);

			Console.WriteLine($"Actual days: {days.Length}");

            var dayCount = days.Length + adjustment;

			if (adjustment != 0M) {
				Console.WriteLine($"Adjusted days: {dayCount}");
            }

			Console.WriteLine($"{dayCount} days @ £{dailyRate} per day");

			Console.WriteLine();

            Console.WriteLine($"{dailyRate * dayCount:C2}");

			Console.WriteLine();

			if (writeDays) {
				foreach (var day in days) {
					Console.WriteLine(day.Date.ToShortDateString());
				}
			}

			if (writeDaysIncludingBankHolidays) { 
				Console.WriteLine("Working days (including bank holidays");
				days = WorkingDaysCalculator.GetWorkingDaysExcludingBankHolidays(year, month);
				Console.WriteLine($"{days.Length} days");
				foreach (var day in days) {
					Console.WriteLine(day.Date.ToShortDateString() + (day.Second == 0 ? null : " (BH)"));
				}
			}

			Console.ReadLine();
        }
    }
}
