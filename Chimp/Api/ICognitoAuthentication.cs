namespace Chimp.Api;

public interface ICognitoAuthentication
{
    Task Login(string userName, string password);
    Task<string?> GetValidBearerToken();
}
