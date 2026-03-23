namespace Chimp.Common;

public static class DateExtensions
{
    extension(DateOnly date)
    {
        public DateOnly WeekStart => date.AddDays(date.DayOfWeek == DayOfWeek.Sunday ? -6 : -(date.DayOfWeek - DayOfWeek.Monday));

        public bool IsCurrentWeek() => date.WeekStart == CurrentWeek;
        public bool IsToday() => date == DateOnly.FromDateTime(DateTime.Today);

        public DateOnly WithWeekDay(DayOfWeek dayOfWeek)
        {
            // Convert DayOfWeek to our system where weeks start on a Monday (Monday = 0, Sunday = 6).
            var currentDayOfWeek = date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - 1;
            var targetDayOfWeek = dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;
            var daysToAdd = targetDayOfWeek - currentDayOfWeek;

            return date.AddDays(daysToAdd);
        }
    }

    public static DateOnly CurrentWeek => DateOnly.FromDateTime(DateTime.Today).WeekStart;
}
