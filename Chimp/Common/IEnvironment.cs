namespace Chimp.Common;

public interface IEnvironment
{
    string ChimpCliDebug => "CHIMPCLI_DEBUG";
    string ChimpCliLanguage => "CHIMPCLI_LANGUAGE";
    string ChimpCliStorePassword => "CHIMPCLI_STORE_PASSWORD";
    string ChimpCliUserName => "CHIMPCLI_USERNAME";
    string ChimpCliPassword => "CHIMPCLI_PASSWORD";

    string[] GetCommandLineArgs();
    string? GetEnvironmentVariable(string variable);
    string GetFolderPath(Environment.SpecialFolder folder);

    bool GetBoolean(string variable) => GetEnvironmentVariable(variable)?.ToLowerInvariant() is not null and not "false" and not "0" and not "no" and not "off";

#if DEBUG
    bool DebugEnabled => true;
#else
    bool DebugEnabled => GetBoolean(ChimpCliDebug);
#endif
}
