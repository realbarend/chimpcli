using System;
using System.Globalization;
using NUnit.Framework;
using Shouldly;

namespace Chimp.Tests;

public class TimeIntervalTests
{
    [TestCase("9-10", 2000, 1, 5, 9, 0, 60, null)]
    [TestCase("09-10", 2000, 1, 5, 9, 0, 60, null)]
    [TestCase("di:9-10", 2000, 1, 4, 9, 0, 60, 1)]
    [TestCase("vr:9-1015", 2000, 1, 7, 9, 0, 75, 4)]
    public void TestHappyFlow(string input, int year, int month, int day, int startHour, int startMinute, int durationMinutes, int? expectedWeekDayNumber)
    {
        var dateBase = DateTime.SpecifyKind(new DateTime(2000, 1, 5), DateTimeKind.Local); // woensdag
        var expectedStart = DateTime.SpecifyKind(new DateTime(year, month, day, startHour, startMinute, 0), DateTimeKind.Local);
        var expectedEnd = expectedStart.AddMinutes(durationMinutes);

        var result = TimeInterval.Parse(input, dateBase, CultureInfo.GetCultureInfo("nl"));
        result.Start.ShouldBe(expectedStart);
        result.End.ShouldBe(expectedEnd);

        TimeInterval.TryParseDayPrefix(input, CultureInfo.GetCultureInfo("nl"), out var weekDayNumber).ShouldBe(expectedWeekDayNumber.HasValue);
        if (expectedWeekDayNumber.HasValue) weekDayNumber.ShouldBe(expectedWeekDayNumber.Value);
    }

    [TestCase("xx:9-10", "detected weekdayprefix but could not parse*")]
    [TestCase("x:9-10", "cannot parse*")]
    [TestCase("10-9", "*end time should be after start time")]
    [TestCase("9+999", "*time interval is not allowed not exceed 10 hours")]
    [TestCase("23+60", "*start and end must be on the same day")]
    [TestCase("24-24", "hour*invalid")]
    [TestCase("960-10", "minute*invalid")]
    public void TestException(string input, string message)
    {
        var dateBase = DateTime.SpecifyKind(new DateTime(2000, 1, 5), DateTimeKind.Local);
        var act = () => TimeInterval.Parse(input, dateBase, CultureInfo.GetCultureInfo("nl"));
        act.ShouldThrow<PebcakException>(message);
    }

    [TestCase("abcd")]
    [TestCase("9")]
    [TestCase("x:9-10")]
    [TestCase("9+1000")]
    public void TestUnparseble(string input)
    {
        var dateBase = DateTime.SpecifyKind(new DateTime(2000, 1, 5), DateTimeKind.Local);

        var act = () => TimeInterval.Parse(input, dateBase, CultureInfo.GetCultureInfo("nl"));
        act.ShouldThrow<Exception>();

        TimeInterval.TryParse(input, dateBase, CultureInfo.GetCultureInfo("nl"), out var interval);
        interval.ShouldBeNull();
    }
}
