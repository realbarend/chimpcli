using System;
using System.IO;
using Chimp.Common;
using Chimp.Services;
using Shouldly;

namespace Chimp.Tests.Common;

public class PersistablePropertyBagTests
{
    [TestCase]
    public void TestSerializeTimeSheetDate()
    {
        var tempFileName = Path.GetTempFileName();
        try
        {
            var newBag = PersistablePropertyBag.CreateNew(tempFileName);
            var date = DateOnly.FromDateTime(DateTime.Now).AddDays(7).WeekStart;
            newBag.Set(new TimeSheetService.TimeSheetDate(date));
            newBag.Save();

            var readBag = PersistablePropertyBag.ReadFromDisk(tempFileName);
            var readValue = readBag.Get<TimeSheetService.TimeSheetDate>();
            readValue?.Date.ShouldBe(date);
        }
        finally
        {
            File.Delete(tempFileName);
        }
    }
}
