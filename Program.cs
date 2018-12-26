using System;

namespace Working_Days_Calculator {
    class Program {
        static void Main(string[] args) {
			int month = 12;
			int year = 2018;
			bool writeDays = true;
			const decimal dailyRate = 344.83M;
			
			var start = new DateTime(year, month, 1);
			var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));

			Console.WriteLine($"Working days in {month}/{year}");
			var days = WorkingDaysCalculator.GetWorkingDays(start, end);

			Console.WriteLine($"{days.Length} days @ £{dailyRate} per day");

			Console.WriteLine();

            Console.WriteLine($"{dailyRate * days.Length:C2}");

			Console.WriteLine();

			if (writeDays) {
				foreach (var day in days) {
					Console.WriteLine(day.Date.ToShortDateString());
				}

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
