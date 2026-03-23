using System;
using Chimp.Common;
using Shouldly;

namespace Chimp.Tests.Common;

public class DateExtensionsTests
{
    [TestCaseSource(nameof(TestFirstDayOfWeekInput))]
    public void TestWeekStart(DateOnly date, DateOnly expected)
    {
        date.WeekStart.ShouldBe(expected);
    }

    [TestCaseSource(nameof(TestSetWeekDayInput))]
    public void TestWithWeekDay(DateOnly date, DayOfWeek dayOfWeek, DateOnly expected)
    {
        date.WithWeekDay(dayOfWeek).ShouldBe(expected);
    }

    public static object[] TestFirstDayOfWeekInput => [
        new object [] { new DateOnly(2025, 11, 3), new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 4), new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 5), new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 6), new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 7), new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 8), new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 9), new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 10),new DateOnly(2025, 11, 10) },
    ];

    public static object[] TestSetWeekDayInput => [
        new object [] { new DateOnly(2025, 11, 3), DayOfWeek.Monday, new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 4), DayOfWeek.Monday, new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 5), DayOfWeek.Monday, new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 6), DayOfWeek.Monday, new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 7), DayOfWeek.Monday, new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 8), DayOfWeek.Monday, new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 9), DayOfWeek.Monday, new DateOnly(2025, 11, 3) },
        new object [] { new DateOnly(2025, 11, 10), DayOfWeek.Monday,new DateOnly(2025, 11, 10) },

        new object [] { new DateOnly(2025, 11, 3), DayOfWeek.Sunday, new DateOnly(2025, 11, 9) },
        new object [] { new DateOnly(2025, 11, 4), DayOfWeek.Sunday, new DateOnly(2025, 11, 9) },
        new object [] { new DateOnly(2025, 11, 5), DayOfWeek.Sunday, new DateOnly(2025, 11, 9) },
        new object [] { new DateOnly(2025, 11, 6), DayOfWeek.Sunday, new DateOnly(2025, 11, 9) },
        new object [] { new DateOnly(2025, 11, 7), DayOfWeek.Sunday, new DateOnly(2025, 11, 9) },
        new object [] { new DateOnly(2025, 11, 8), DayOfWeek.Sunday, new DateOnly(2025, 11, 9) },
        new object [] { new DateOnly(2025, 11, 9), DayOfWeek.Sunday, new DateOnly(2025, 11, 9) },
        new object [] { new DateOnly(2025, 11, 10),DayOfWeek.Sunday, new DateOnly(2025, 11, 16) },

        new object [] { new DateOnly(2025, 11, 3), DayOfWeek.Saturday, new DateOnly(2025, 11, 8) },
        new object [] { new DateOnly(2025, 11, 4), DayOfWeek.Saturday, new DateOnly(2025, 11, 8) },
        new object [] { new DateOnly(2025, 11, 5), DayOfWeek.Saturday, new DateOnly(2025, 11, 8) },
        new object [] { new DateOnly(2025, 11, 6), DayOfWeek.Saturday, new DateOnly(2025, 11, 8) },
        new object [] { new DateOnly(2025, 11, 7), DayOfWeek.Saturday, new DateOnly(2025, 11, 8) },
        new object [] { new DateOnly(2025, 11, 8), DayOfWeek.Saturday, new DateOnly(2025, 11, 8) },
        new object [] { new DateOnly(2025, 11, 9), DayOfWeek.Saturday, new DateOnly(2025, 11, 8) },
        new object [] { new DateOnly(2025, 11, 10),DayOfWeek.Saturday, new DateOnly(2025, 11, 15) },
    ];
}
