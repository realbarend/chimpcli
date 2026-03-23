using System;
using System.Collections;
using System.Globalization;
using Chimp.Common;
using Chimp.DomainModels;
using Shouldly;

namespace Chimp.Tests.DomainModels;

[TestFixture]
public class TimeEntryTests
{
    [TestCaseSource(nameof(TryParseShouldParseCorrectlyCases))]
    public void TryParseShouldParseCorrectly(string input, string canonical, TimeOnly? expectedStart, TimeSpan expectedDuration, DayOfWeek? expectedDayOfWeek)
    {
        using var _ = new UiCultureMocker(CultureInfo.GetCultureInfo("nl"));

        var success = TimeEntry.TryParse(input, out var timeEntry);
        success.ShouldBeTrue();
        timeEntry.ShouldNotBeNull();

        timeEntry.ToCanonicalString().ShouldBe(canonical);
        timeEntry.StartTime.ShouldBe(expectedStart);
        timeEntry.Duration.ShouldBe(expectedDuration);
        timeEntry.DayOfWeek.ShouldBe(expectedDayOfWeek);
    }

    public static IEnumerable TryParseShouldParseCorrectlyCases
    {
        get
        {
            yield return new TestCaseData("9-10", "9-10", new TimeOnly(9, 0), TimeSpan.FromHours(1), null);
            yield return new TestCaseData("09-10", "9-10", new TimeOnly(9, 0), TimeSpan.FromHours(1), null);
            yield return new TestCaseData("di:9-1000", "di:9-10", new TimeOnly(9, 0), TimeSpan.FromHours(1), DayOfWeek.Tuesday);
            yield return new TestCaseData("woensdag:9-1000", "wo:9-10", new TimeOnly(9, 0), TimeSpan.FromHours(1), DayOfWeek.Wednesday);
            yield return new TestCaseData("wed:9-1000", "wo:9-10", new TimeOnly(9, 0), TimeSpan.FromHours(1), DayOfWeek.Wednesday);
            yield return new TestCaseData("vr:9-1015", "vr:9-1015", new TimeOnly(9, 0), TimeSpan.FromMinutes(75), DayOfWeek.Friday);
            yield return new TestCaseData("vrij:9-1015", "vr:9-1015", new TimeOnly(9, 0), TimeSpan.FromMinutes(75), DayOfWeek.Friday);

            yield return new TestCaseData("1h", "1h", null, TimeSpan.FromHours(1), null);
            yield return new TestCaseData("90m", "1h30m", null, TimeSpan.FromMinutes(90), null);
            yield return new TestCaseData("1h30m", "1h30m", null, TimeSpan.FromMinutes(90), null);
            yield return new TestCaseData("0h", "0m", null, TimeSpan.Zero, null);
        }
    }

    [TestCase("xx:9-10", "detected weekdayprefix but could not parse*")]
    [TestCase("10-9", "*end time should be after start time")]
    [TestCase("9+999", "*time interval is not allowed not exceed 10 hours")]
    [TestCase("23+60", "*start and end must be on the same day")]
    [TestCase("24-24", "hour*invalid")]
    [TestCase("960-10", "minute*invalid")]
    public void SuccessfulParseShouldThrowWhenNotAllowed(string input, string message)
    {
        var act = () => TimeEntry.TryParse(input, out _).ShouldBeFalse();
        act.ShouldThrow<Error>(message);
    }

    [TestCase("abcd")]
    [TestCase("9")]
    [TestCase("")]
    [TestCase("9+1000")]
    public void TestUnparsebleReturnsFalse(string input)
    {
        TimeEntry.TryParse(input, out _).ShouldBeFalse();
    }

    private class UiCultureMocker : IDisposable
    {
        private readonly CultureInfo _originalCulture;

        public UiCultureMocker(CultureInfo mockCulture)
        {
            _originalCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = mockCulture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentUICulture = _originalCulture;
        }
    }
}
