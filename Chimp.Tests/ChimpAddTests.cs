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
    public async Task Test()
    {
        var service = new Mock<IChimpService>();
        service.Setup(s => s.GetLocalizer()).Returns(new Localizer("nl"));

        await new ChimpAdd(new ArgumentShifter(["p12-34", "9-10", "the comments"]), service.Object).Run();

        service.Verify(s => s.AddRow(
            12,
            It.Is<IEnumerable<int>>(list => list.SequenceEqual(new[] { 34 }) ),
            It.Is<TimeInterval>(ti => ti.Start.Hour == 9),
            It.Is<string>(s => s == "the comments")
        ), Times.Once);
        service.Invocations.Where(inv => inv.Method.Name == nameof(IChimpService.AddRow)).ShouldHaveSingleItem();
    }
}
