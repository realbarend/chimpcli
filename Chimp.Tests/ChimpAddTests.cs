using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Chimp.Tests;

public class ChimpAddTests
{
    [TestCase]
    public async Task TestAddHappyFlow()
    {
        var service = new Mock<IChimpService>();
        service.Setup(s => s.GetLocalizer()).Returns(new Localizer("nl"));

        // flipping TimeSpec and Notes should have the exact same result
        await new ChimpAdd(new ArgumentShifter(["p12-34", "9-10", "the", "comments"]), service.Object).Run();
        await new ChimpAdd(new ArgumentShifter(["p12-34", "the", "comments", "9-10"]), service.Object).Run();

        service.Verify(s => s.AddRow(
            12,
            It.Is<IEnumerable<int>>(list => list.SequenceEqual(new[] { 34 }) ),
            It.Is<TimeInterval>(ti => ti.Start.Hour == 9),
            It.Is<string>(s => s == "the comments")
        ), Times.Exactly(2));
        service.Invocations.Count(inv => inv.Method.Name == nameof(IChimpService.AddRow)).ShouldBe(2);
    }

    [TestCase]
    public async Task TestOnlyTimeSpec()
    {
        var service = new Mock<IChimpService>();
        service.Setup(s => s.GetLocalizer()).Returns(new Localizer("nl"));

        await new ChimpAdd(new ArgumentShifter(["p12-34", "9-10"]), service.Object).Run();

        service.Verify(s => s.AddRow(
            12,
            It.Is<IEnumerable<int>>(list => list.SequenceEqual(new[] { 34 }) ),
            It.Is<TimeInterval>(ti => ti.Start.Hour == 9),
            It.Is<string>(s => s == string.Empty)
        ), Times.Once);
        service.Invocations.Count(inv => inv.Method.Name == nameof(IChimpService.AddRow)).ShouldBe(1);
    }
}
