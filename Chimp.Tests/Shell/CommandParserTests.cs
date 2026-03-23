using System;
using System.IO;
using System.Linq;
using Chimp.Common;
using Chimp.Shell;
using Moq;
using Shouldly;

namespace Chimp.Tests.Shell;

public class CommandParserTests
{
    [TestCase(typeof(LoginCommand), "l", "login")]
    [TestCase(typeof(ListProjectsCommand), "p", "projects")]
    [TestCase(typeof(ListTimeSheetCommand), "ls", "list")]
    [TestCase(typeof(TimeTravelerCommand), "w", "week")]
    [TestCase(typeof(AddTimeSheetRowCommand), "a p1 9-10", "add p1 9-10")]
    [TestCase(typeof(UpdateTimeSheetRowCommand), "u 1 p1", "update 1 p1")]
    [TestCase(typeof(DeleteTimeSheetRowCommand), "d 1", "del 1", "delete 1")]
    public void TestCommandMapping(Type expectedType, params string[] commandLines)
    {
        foreach (var commandLine in commandLines)
        {
            var command = GetTerminalParser(commandLine).ParseCommandLine();
            command.ShouldBeOfType(expectedType);
        }
    }

    [Test]
    public void TestAddTimeSheetRowCommand()
    {
        var command1 = GetTerminalParser("a p12-34 9-10 the comments").ParseCommandLine();
        var command2 = GetTerminalParser("a p12-34 the comments 9-10").ParseCommandLine();

        command1.ShouldBeEquivalentTo(command2, "flipping TimeEntry and Notes should have the exact same result");

        command1.ShouldBeOfType<AddTimeSheetRowCommand>();
        var command = (AddTimeSheetRowCommand)command1;

        command.Project.Value.ShouldBe(12);
        command.Tags.Select(t => t.Value).ShouldBe([34]);
        command.TimeEntry.ToCanonicalString().ShouldBe("9-10");
        command.TimeEntry.StartTime.ShouldBe(new TimeOnly(9, 0));
        command.TimeEntry.Duration.ShouldBe(TimeSpan.FromHours(1));
        command.Notes.ShouldBe("the comments");
    }

    [Test]
    public void TestUpdateTimeSheetRowCommand()
    {
        var updateProject = GetTerminalParser("u 123 p12-34").ParseCommandLine();
        updateProject.ShouldBeOfType<UpdateTimeSheetRowCommand>();
        ((UpdateTimeSheetRowCommand)updateProject).ShortId.Value.ShouldBe(123);
        ((UpdateTimeSheetRowCommand)updateProject).Project.ShouldNotBeNull();
        ((UpdateTimeSheetRowCommand)updateProject).TimeEntry.ShouldBeNull();
        ((UpdateTimeSheetRowCommand)updateProject).Notes.ShouldBeNull();

        var updateTimeEntry = GetTerminalParser("u 123 9-10").ParseCommandLine();
        updateTimeEntry.ShouldBeOfType<UpdateTimeSheetRowCommand>();
        ((UpdateTimeSheetRowCommand)updateTimeEntry).ShortId.Value.ShouldBe(123);
        ((UpdateTimeSheetRowCommand)updateTimeEntry).Project.ShouldBeNull();
        ((UpdateTimeSheetRowCommand)updateTimeEntry).TimeEntry.ShouldNotBeNull();
        ((UpdateTimeSheetRowCommand)updateTimeEntry).TimeEntry!.ToCanonicalString().ShouldBe("9-10");
        ((UpdateTimeSheetRowCommand)updateTimeEntry).Notes.ShouldBeNull();

        var updateNotes = GetTerminalParser("u 123 test").ParseCommandLine();
        updateNotes.ShouldBeOfType<UpdateTimeSheetRowCommand>();
        ((UpdateTimeSheetRowCommand)updateNotes).ShortId.Value.ShouldBe(123);
        ((UpdateTimeSheetRowCommand)updateNotes).Project.ShouldBeNull();
        ((UpdateTimeSheetRowCommand)updateNotes).TimeEntry.ShouldBeNull();
        ((UpdateTimeSheetRowCommand)updateNotes).Notes.ShouldNotBeNull();
        ((UpdateTimeSheetRowCommand)updateNotes).Notes!.ShouldBe("test");
    }

    [Test]
    public void TestDeleteCommand()
    {
        var command = GetTerminalParser("delete 123").ParseCommandLine();
        command.ShouldBeOfType<DeleteTimeSheetRowCommand>();
        var deleteCommand = (DeleteTimeSheetRowCommand)command;
        deleteCommand.ShortId.Value.ShouldBe(123);
    }

    [Test]
    public void TestLoginCommand()
    {
        var command = GetTerminalParser("login").ParseCommandLine();
        command.ShouldBeOfType<LoginCommand>();
        var loginCommand = (LoginCommand)command;
        loginCommand.User.ShouldBe("testuser");
        loginCommand.Password.ShouldBe("testpassword");
        loginCommand.PersistCredentials.ShouldBeTrue();
    }

    private static CommandParser GetTerminalParser(string commandLine)
        => new(new TestEnvironment(commandLine), new DebugLogger(new Mock<TextWriter>().Object));

    private class TestEnvironment(string commandLine) : IEnvironment
    {
        public string[] GetCommandLineArgs() => [
            "chimp.exe",
            ..commandLine.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
        ];

        public string? GetEnvironmentVariable(string variable) => variable switch
            {
                "CHIMPCLI_USERNAME" => "testuser",
                "CHIMPCLI_PASSWORD" => "testpassword",
                "CHIMPCLI_STORE_PASSWORD" => "true",
                _ => null,
            };

        public string GetFolderPath(Environment.SpecialFolder folder) => throw new NotImplementedException();
    }
}
