using Chimp.Common;

namespace Chimp.Shell;

public class SystemEnvironment : IEnvironment
{
    public string[] GetCommandLineArgs() => Environment.GetCommandLineArgs();

    public string? GetEnvironmentVariable(string variable) => Environment.GetEnvironmentVariable(variable);

    public string GetFolderPath(Environment.SpecialFolder folder) => Environment.GetFolderPath(folder);
}
