using System.Linq;
using Chimp.Shell;
using Shouldly;

namespace Chimp.Tests.Shell;

[TestFixture]
public class RenderHelperTests
{
    [TestCase("p1", 1, new int[] { })]
    [TestCase("p1-23", 1, new[] { 23 })]
    [TestCase("p1-23,44", 1, new[] { 23, 44 })]
    [TestCase("p1-23,44,33", 1, new[] { 23, 44, 33 })]
    [TestCase("p5-1,2,3", 5, new[] { 1, 2, 3 })]
    [TestCase("p10-99", 10, new[] { 99 })]
    public void ValidPatterns(string input, int expectedP, int[] expectedArray)
    {
        var result = RenderHelper.TryParseProjectAlias(input, out var project, out var tags);

        result.ShouldBeTrue();
        project!.Value.ShouldBe(expectedP);
        tags!.Select(v => v.Value).ShouldBe(expectedArray);
    }

    [TestCase("p1-,23")]
    [TestCase("p1-23,44,")]
    [TestCase("p1-,23,44,33,")]
    [TestCase("p-1,2,3")]
    [TestCase("p1-23,,44")]
    [TestCase("p1-abc,23")]
    [TestCase("p1-23,4,5,,6")]
    public void InvalidPatterns(string input)
    {
        var result = RenderHelper.TryParseProjectAlias(input, out _, out _);

        result.ShouldBeFalse();
    }
}
