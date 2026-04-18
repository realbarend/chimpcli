using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Chimp.Common;
using Chimp.Shell;
using Shouldly;

namespace Chimp.Tests.Shell;

[TestFixture]
public class LocalizationTests
{
    [OneTimeSetUp]
    public void Setup()
    {
        Localization.NlTranslations.Add("without params", "zonder params");
        Localization.NlTranslations.Add("EN {param1} b {param2} c", "NL {param1} b {param2} c");
        Localization.NlTranslations.Add("EN order {param1} {param2}", "NL order {param2} {param1}");
        Localization.NlTranslations.Add("EN repeat {param1} {param2} {param1}", "NL repeat {param1} {param2} {param1}");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Localization.NlTranslations.Remove("X");
    }

    [TestCase("en", "without params",                       "--",      "--", "without params")]
    // translation translates without parameters
    [TestCase("nl", "without params",                       "--",      "--", "zonder params")]
    // parameters replace placeholders
    [TestCase("en", "A {param1} b {param2} c",              "value1",  123,  "A value1 b 123 c")]
    // translation translates with parameters
    [TestCase("nl", "EN {param1} b {param2} c",             "value1",  123,  "NL value1 b 123 c")]
    [TestCase("en", "EN order {param1} {param2}",           "value1",  123,  "EN order value1 123")]
    // translation may re-order params
    [TestCase("nl", "EN order {param1} {param2}",           "value1",  123,  "NL order 123 value1")]
    // repeated params are allowed
    [TestCase("nl", "EN repeat {param1} {param2} {param1}", "value1",  123,  "NL repeat value1 123 value1")]
    // extra parameters are added at the end
    [TestCase("nl", "without params",                       "value1",  123,  "zonder params (value1, 123)")]
    // null values resolve to empty string
    [TestCase("nl", "EN {param1} b {param2} c",              "",  null,  "NL  b  c")]
    public void Localize(string culture, string literal, object? arg1, object? arg2, string expected)
    {
        var args = new[] { arg1, arg2 }.Where(a => a is not "--").ToArray();
        using IDisposable? _ = culture == "nl" ? new DutchCulture() : null;
        Localization.Localize(literal, args).ShouldBe(expected);
    }

    [Test]
    public void ValidateThatAllNlTranslationsHaveSamePlaceholdersAsEnglishOriginal()
    {
        foreach (var (english, dutch) in Localization.NlTranslations)
        {
            var englishPlaceholders = Localization.GetOrderedPlaceholders(english);
            var translationPlaceholders = Localization.GetOrderedPlaceholders(dutch);
            translationPlaceholders.ShouldBe(englishPlaceholders, ignoreOrder: true,
                $"Dutch translation '{dutch}' has different placeholders than English original '{english}'");
        }
    }

    [Test]
    public void WriteLocalized_MapsParametersAndWritesToConsole()
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try { Localization.WriteLocalized("EN {param1} b {param2} c", "abc", 123); }
        finally { Console.SetOut(original); }
        writer.ToString().Trim().ShouldBe("EN abc b 123 c");
    }

    [Test]
    public void Error_MapsParametersAndLocalizes()
    {
        using var _ = new DutchCulture();
        var error = new Error("EN {param1} b {param2} c", 123, "abc");
        Localization.Localize(error.Message, error.Args).ShouldBe("NL 123 b abc c");
    }

    private sealed class DutchCulture : IDisposable
    {
        private readonly CultureInfo _original = CultureInfo.CurrentUICulture;
        public DutchCulture() => CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("nl");
        public void Dispose() => CultureInfo.CurrentUICulture = _original;
    }
}
