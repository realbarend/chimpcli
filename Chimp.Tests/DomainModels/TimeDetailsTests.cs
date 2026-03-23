using System;
using System.Reflection;
using Chimp.DomainModels;
using Shouldly;

namespace Chimp.Tests.DomainModels;

[TestFixture]
public class TimeDetailsTests
{
    [Test]
    public void TestItCalculatesCorrectly()
    {
        var timeDifference = TimeSpan.FromHours(6);
        using var _ = new LocalTimeZoneInfoMocker(TimeZoneInfo.CreateCustomTimeZone("FakeZone", timeDifference, "FakeZone", "FakeZone"));

        var date = new DateOnly(2025, 11, 10);
        var time = new TimeOnly(12, 30);
        var expected = new DateTime(2025, 11, 11, 12, 30, 0);

        var sut = TimeDetails.FromTimeEntry(new TimeEntry { StartTime = time, Duration = TimeSpan.FromMinutes(75), DayOfWeek = DayOfWeek.Tuesday }, new TimeSheet([], [], [], date));

        sut.Date.ShouldBe(DateOnly.FromDateTime(expected));

        sut.Start.ShouldBe(expected);
        sut.Start.HasValue.ShouldBeTrue();
        sut.Start.Value.Kind.ShouldBe(DateTimeKind.Local);
        // Converting to local time should not change the value, because Kind is already Local.
        sut.Start.Value.ToLocalTime().ShouldBe(expected);
        // However, converting to UTC _should_ change the value, with exactly the expected offset.
        sut.Start.Value.ToUniversalTime().ShouldBe(expected - timeDifference);

        sut.End.HasValue.ShouldBeTrue();
        sut.End.Value.Kind.ShouldBe(DateTimeKind.Local);
        sut.End.ShouldBe(sut.Start.Value.AddMinutes(75));

        sut.Hours.ShouldBe(1.25);
    }

    [Test]
    public void TestThatTimeEntryWithOnlyDurationResultsInNoStartAndNoEnd()
    {
        var sut = TimeDetails.FromTimeEntry(new TimeEntry { Duration = TimeSpan.FromMinutes(75) }, new TimeSheet([], [], [], DateOnly.FromDateTime(DateTime.Now)));

        sut.Date.ShouldBe(DateOnly.FromDateTime(DateTime.Now));

        sut.Start.ShouldBe(null);
        sut.End.ShouldBe(null);

        sut.Hours.ShouldBe(1.25);
    }

    [Test]
    public void TestItRequiresDayOfWeekWhenNotCurrentWeek()
    {
        Should.Throw<Chimp.Common.Error>(() => TimeDetails.FromTimeEntry(new TimeEntry { StartTime = new TimeOnly(9, 0), Duration = TimeSpan.Zero }, new TimeSheet([], [], [], DateOnly.FromDateTime(DateTime.Now.AddDays(7)))));
    }

    private class LocalTimeZoneInfoMocker : IDisposable
    {
        public LocalTimeZoneInfoMocker(TimeZoneInfo mockTimeZoneInfo)
        {
            try
            {
                var info = typeof(TimeZoneInfo).GetField("s_cachedData", BindingFlags.NonPublic | BindingFlags.Static);
                var cachedData = info!.GetValue(null);
                var field = cachedData!.GetType().GetField("_localTimeZone",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Instance);
                field!.SetValue(cachedData, mockTimeZoneInfo);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to mock local timezone: probably due to changed dotnet internals", e);
            }
        }

        public void Dispose()
        {
            TimeZoneInfo.ClearCachedData();
        }
    }
}
