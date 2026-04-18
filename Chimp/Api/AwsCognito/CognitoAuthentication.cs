using System.Text.RegularExpressions;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using Chimp.Common;

namespace Chimp.Api.AwsCognito;

public class CognitoAuthentication(PersistablePropertyBag stateBag, HttpClient httpClient, DebugLogger logger) : ICognitoAuthentication
{
    private record Environment(string UserPoolId, string ClientId);
    private record AuthenticationSession(AmazonCognitoIdentityProviderClient Provider, Environment Environment, CognitoUser User, AuthFlowResponse AuthenticationResponse);

    private record Credentials(
        Environment Environment,
        string UserName,
        string AccessToken,
        DateTimeOffset Expires,
        string RefreshToken)
    {
        public static Credentials FromAuthenticationResult(Environment environment, string userName, AuthenticationResultType auth)
            => new(environment, userName, auth.AccessToken, DateTimeOffset.UtcNow.AddSeconds(auth.ExpiresIn ?? 0), auth.RefreshToken);
    }

    /// <exception cref="Exception">when login somehow fails</exception>
    public async Task Login(string userName, string password)
    {
        var environment = await FindEnvironment();
        using var provider = CreateIdentityProviderClient(environment);
        var userPool = new CognitoUserPool(environment.UserPoolId, environment.ClientId, provider);
        var user = new CognitoUser(userName, environment.ClientId, userPool, provider);
        var response = await user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest { Password = password });
        var authenticationSession = new AuthenticationSession(provider, environment, user, response);
        var authenticationResult = await HandleMfaChallenge(authenticationSession);
        var credentials = Credentials.FromAuthenticationResult(environment, userName, authenticationResult);
        stateBag.Set(credentials);

        logger.Log($"** login successful, got accesstoken valid until {credentials.Expires.ToLocalTime():D}, will automatically refresh using refreshtoken");
    }

    private static async Task<AuthenticationResultType> HandleMfaChallenge(AuthenticationSession session)
    {
        // No challenge means that 2fa is not enabled.
        if (session.AuthenticationResponse.ChallengeName == null) return session.AuthenticationResponse.AuthenticationResult;

        if (session.AuthenticationResponse.ChallengeName == ChallengeNameType.SOFTWARE_TOKEN_MFA) return await ProcessSoftwareTokenChallenge(session);

        throw new Error("could not finish login: unsupported challenge: {Challenge}", session.AuthenticationResponse.ChallengeName.Value);
    }

    private static async Task<AuthenticationResultType> ProcessSoftwareTokenChallenge(AuthenticationSession session)
    {
        Console.Write("Enter your 2FA code: ");
        var softwareToken = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(softwareToken)) throw new Error("2fa code empty: cannot finish login");

        var response = await session.Provider.RespondToAuthChallengeAsync(new RespondToAuthChallengeRequest
        {
            ChallengeName = session.AuthenticationResponse.ChallengeName,
            ClientId = session.Environment.ClientId,
            ChallengeResponses = new Dictionary<string, string>
            {
                { "USERNAME", session.User.UserID },
                { "SOFTWARE_TOKEN_MFA_CODE", softwareToken },
            },
            Session = session.AuthenticationResponse.SessionID,
        });
        return response.AuthenticationResult;
    }

    /// <exception cref="Exception">if the current token expired and could not be refreshed</exception>
    public async Task<string?> GetValidBearerToken()
    {
        var credentials = stateBag.Get<Credentials>();
        if (credentials == null) return null;

        if (credentials.Expires <= DateTimeOffset.UtcNow.AddSeconds(60)) credentials = await RefreshCredentials(credentials);

        return credentials.AccessToken;
    }

    private AmazonCognitoIdentityProviderClient CreateIdentityProviderClient(Environment env)
    {
        return new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), new AmazonCognitoIdentityProviderConfig
        {
            HttpClientFactory = new CognitoHttpClientFactory(httpClient),
            RegionEndpoint = RegionEndpoint.GetBySystemName(env.UserPoolId.Split('_')[0]),
        });
    }

    private class CognitoHttpClientFactory(HttpClient client) : HttpClientFactory
    {
        public override HttpClient CreateHttpClient(IClientConfig config) => client;
    }

    private async Task<Environment> FindEnvironment()
    {
        try
        {
            // Lookup the cognito user-pool and client-id by peeking in the javascript files that are loaded by the app.timechimp.com website.
            // This is not very pretty, but for the time being it gets the job done.

            var web = "https://app.timechimp.com";
            var html = await httpClient.GetStringAsync($"{web}/auth/prelogin");
            foreach (var script in Regex.Matches(html, """ src="(/_next/static/chunks/[0-9a-f]{16}.js)" """).Select(s => s.Groups[1].Value))
            {
                logger.Log($"** searching cognito environment in {web}{script}");
                html = await httpClient.GetStringAsync($"https://app.timechimp.com{script}");
                var poolmatch = Regex.Match(html, @"userPoolId:\s*""(?<pool>[^""]+)""");
                var clientmatch = Regex.Match(html, @"userPoolClientId:\s*""(?<client>[^""]+)""");
                if (!poolmatch.Success || !clientmatch.Success) continue;

                var pool = poolmatch.Groups["pool"].Value;
                var client = clientmatch.Groups["client"].Value;

                logger.Log($"** found cognito environment: userPoolId={pool}, clientId={client}");
                return new Environment(pool, client);
            }
            throw new ApplicationException("unable to guess the aws cognito environment");
        }
        catch (Exception e)
        {
            throw new ApplicationException($"unable to guess the aws cognito environment: {e.Message}");
        }
    }

    private async Task<Credentials> RefreshCredentials(Credentials credentials)
    {
        try
        {
            using var provider = CreateIdentityProviderClient(credentials.Environment);
            var userPool = new CognitoUserPool(credentials.Environment.UserPoolId, credentials.Environment.ClientId, provider);
            var user = new CognitoUser(credentials.UserName, credentials.Environment.ClientId, userPool, provider)
                { SessionTokens = new CognitoUserSession(null, null, credentials.RefreshToken, default, default) };
            var authFlowResponse = await user.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest { AuthFlowType = AuthFlowType.REFRESH_TOKEN });

            credentials = Credentials.FromAuthenticationResult(credentials.Environment, credentials.UserName, authFlowResponse.AuthenticationResult);
            stateBag.Set(credentials);

            logger.Log($"** refresh: got new accesstoken valid until {credentials.Expires.ToLocalTime():D}");
            return credentials;
        }
        catch (Exception e)
        {
            throw new Error("** refresh: refreshing the accesstoken failed: {Message}", e.Message);
        }
    }
}
