namespace Chimp.Common;

public class DebugLogger(TextWriter writer)
{
    public void Log(string message) => writer.WriteLine(message);
}
