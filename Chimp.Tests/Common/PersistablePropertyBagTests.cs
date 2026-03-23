using System;
using System.IO;
using Chimp.Common;
using Shouldly;

namespace Chimp.Tests.Common;

public class PersistablePropertyBagTests
{
    private record TestObject(DateOnly Date);

    [TestCase]
    public void TestSerializeTimeSheetDate()
    {
        var tempFileName = Path.GetTempFileName();
        try
        {
            var newBag = PersistablePropertyBag.CreateNew(tempFileName);
            var date = DateOnly.FromDateTime(DateTime.Now).AddDays(7).WeekStart;
            newBag.Set(new TestObject(date));
            newBag.Save();

            var readBag = PersistablePropertyBag.ReadFromDisk(tempFileName);
            var readValue = readBag.Get<TestObject>();
            readValue?.Date.ShouldBe(date);
        }
        finally
        {
            File.Delete(tempFileName);
        }
    }
}
