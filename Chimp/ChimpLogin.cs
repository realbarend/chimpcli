namespace Chimp;

public class ChimpLogin(ChimpService service)
{
    public async Task Run()
    {
        var persistCredentialsEnv = Environment.GetEnvironmentVariable("CHIMPCLI_PERSIST_LOGIN_CREDENTIALS")?.ToLowerInvariant();
        var persistCredentials = persistCredentialsEnv is "true" or "1" or "yes" or "yes, please";

        var username = Environment.GetEnvironmentVariable("CHIMPCLI_USERNAME");
        if (!string.IsNullOrEmpty(username)) Console.WriteLine("Reading your timechimp username from env.CHIMPCLI_USERNAME");
        var password = Environment.GetEnvironmentVariable("CHIMPCLI_PASSWORD");
        if (!string.IsNullOrEmpty(password)) Console.WriteLine("Reading your timechimp password from env.CHIMPCLI_PASSWORD");
        
        // if credentials were passed via ENV, then we prefer to use those for login
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            await service.DoLogin(username, password, persistCredentials);
            return;
        }

        // next: if we have previously persisted credentials, use those
        if (await service.DoLoginIfPersistedCredentials()) return;

        // otherwise: do (partial) interactive login
        if (string.IsNullOrEmpty(username))
        {
            Console.Write("Enter your timechimp username: ");
            username = Console.ReadLine();
            if (string.IsNullOrEmpty(username)) throw new PebcakException("username empty: cannot login");
        }
        if (string.IsNullOrEmpty(password))
        {
            Console.Write("Enter your timechimp password: ");
            password = Console.ReadLine();
            if (string.IsNullOrEmpty(password)) throw new PebcakException("password empty: cannot login");
        }

        await service.DoLogin(username, password, persistCredentials);
    }
}
