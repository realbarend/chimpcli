using System;
using System.Linq;
using System.Threading.Tasks;
using Chimp.Models;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Chimp.Tests;

public class ChimpUpdateTests
{
    [TestCase]
    public async Task TestUpdateTimeSpec()
    {
        var service = new Mock<IChimpService>();
        service.Setup(s => s.GetLocalizer()).Returns(new Localizer("nl"));
        service.Setup(s => s.GetCachedTimeSheetViewRow(It.Is<int>(x => x == 5)))
            .Returns(new TimeSheetRowViewModel(0, DateTime.Now, null, null, 0, string.Empty, string.Empty, string.Empty, string.Empty, null!));

        await new ChimpUpdate(new ArgumentShifter(["5", "19-20"]), service.Object).Run();

        service.Verify(s => s.UpdateTimeInterval(
            5,
            It.Is<TimeInterval>(ti => ti.Start.Hour == 19)
        ), Times.Once);
        service.Invocations.Where(inv => inv.Method.Name == nameof(IChimpService.UpdateTimeInterval)).ShouldHaveSingleItem();
        service.Invocations.ShouldNotContain(inv => inv.Method.Name == nameof(IChimpService.UpdateNotes));
        service.Invocations.ShouldNotContain(inv => inv.Method.Name == nameof(IChimpService.UpdateProject));
    }

    [TestCase]
    public async Task TestUpdateNotes()
    {
        var service = new Mock<IChimpService>();
        service.Setup(s => s.GetLocalizer()).Returns(new Localizer("nl"));
        service.Setup(s => s.GetCachedTimeSheetViewRow(It.Is<int>(x => x == 5)))
            .Returns(new TimeSheetRowViewModel(0, DateTime.Now, null, null, 0, string.Empty, string.Empty, string.Empty, string.Empty, null!));

        await new ChimpUpdate(new ArgumentShifter(["5", "boe"]), service.Object).Run();

        service.Verify(s => s.UpdateNotes(
            5,
            It.Is<string>(s => s == "boe")
        ), Times.Once);
        service.Invocations.Where(inv => inv.Method.Name == nameof(IChimpService.UpdateNotes)).ShouldHaveSingleItem();
        service.Invocations.ShouldNotContain(inv => inv.Method.Name == nameof(IChimpService.UpdateProject));
        service.Invocations.ShouldNotContain(inv => inv.Method.Name == nameof(IChimpService.UpdateTimeInterval));
    }
}
