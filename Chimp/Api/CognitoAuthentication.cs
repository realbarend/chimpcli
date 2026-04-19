using Chimp.Api.AwsCognito;
using Chimp.Common;

namespace Chimp.Api;

public class CognitoAuthentication(PersistablePropertyBag stateBag, HttpClient httpClient, DebugLogger logger) : ICognitoAuthentication
{
    private const string UserPoolId = "eu-central-1_NIGD97pvM";
    private const string ClientId = "7nq87ee95nibpha7loi8pb7lnp";

    internal record Credentials(string UserName, string AccessToken, DateTimeOffset Expires, string RefreshToken);

    private readonly CognitoApiClient _api = new(httpClient, UserPoolId, ClientId);

    /// <exception cref="Exception">when login somehow fails</exception>
    public async Task Login(string userName, string password)
    {
        var auth = await _api.AuthenticateSrp(userName, password);

        CognitoTokens cognitoTokens;
        if (!auth.RequiresMfa) cognitoTokens = auth.Tokens;
        else
        {
            Console.Write("Enter your 2FA code: ");
            var code = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(code)) throw new Error("2fa code empty: cannot finish login");
            cognitoTokens = await _api.CompleteMfaChallenge(auth.MfaChallenge, code);
        }

        var credentials = new Credentials(
            userName,
            cognitoTokens.AccessToken,
            DateTimeOffset.UtcNow.AddSeconds(cognitoTokens.ExpiresIn),
            cognitoTokens.RefreshToken ?? throw new InvalidOperationException("Cognito did not return a refresh token"));
        stateBag.Set(credentials);
        logger.Log($"** login successful, got accesstoken valid until {credentials.Expires.ToLocalTime():D}, will automatically refresh using refreshtoken");
    }

    /// <exception cref="Exception">if the current token expired and could not be refreshed</exception>
    public async Task<string?> GetValidBearerToken()
    {
        var credentials = stateBag.Get<Credentials>();
        if (credentials == null) return null;

        if (credentials.Expires <= DateTimeOffset.UtcNow.AddSeconds(60)) credentials = await RefreshCredentials(credentials);
        return credentials.AccessToken;
    }

    private async Task<Credentials> RefreshCredentials(Credentials credentials)
    {
        try
        {
            var tokens = await _api.RefreshToken(credentials.RefreshToken);
            var refreshed = new Credentials(
                credentials.UserName,
                tokens.AccessToken,
                DateTimeOffset.UtcNow.AddSeconds(tokens.ExpiresIn),
                tokens.RefreshToken ?? credentials.RefreshToken);
            stateBag.Set(refreshed);
            logger.Log($"** refresh: got new accesstoken valid until {refreshed.Expires.ToLocalTime():D}");
            return refreshed;
        }
        catch (Exception e)
        {
            throw new Error("** refresh: refreshing the accesstoken failed: {Message}", e.Message);
        }
    }
}
