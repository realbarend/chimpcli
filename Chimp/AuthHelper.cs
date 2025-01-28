using System.Text.RegularExpressions;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Chimp.Models;

namespace Chimp;

public static class AuthHelper
{
    /// <exception cref="Exception">when login somehow fails</exception>
    public static async Task<AuthResult> Login(string userName, string userPassword)
    {
        var environment = await GetCognitoEnvironment();
        using var provider = CreateIdentityProviderClient(environment);
        var userPool = new CognitoUserPool(environment.UserPoolId, environment.ClientId, provider);
        var user = new CognitoUser(userName, environment.ClientId, userPool, provider);
        var authFlowResponse = await user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest { Password = userPassword });
        return AuthResult.FromAuthenticationResultType(authFlowResponse.AuthenticationResult, environment);
    }

    public static async Task<AuthResult?> TryRefresh(AuthProperties auth)
    {
        try
        {
            var enviroment = new CognitoEnvironment(auth.CognitoClientId, auth.CognitoUserPoolId);
            using var provider = CreateIdentityProviderClient(enviroment);
            var userPool = new CognitoUserPool(auth.CognitoUserPoolId, auth.CognitoClientId, provider);
            var user = new CognitoUser(auth.LoginUserName, auth.CognitoClientId, userPool, provider) {
                SessionTokens = new CognitoUserSession(null, null, auth.RefreshToken, default, default),
            };
            var authFlowResponse = await user.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest { AuthFlowType = AuthFlowType.REFRESH_TOKEN });
            return AuthResult.FromAuthenticationResultType(authFlowResponse.AuthenticationResult, enviroment);
        }
        catch (Exception e)
        {
            Console.WriteLine($"** tried to refresh your accesstoken but miserably failed: {e.Message}");
            return null;
        }
    }

    private static AmazonCognitoIdentityProviderClient CreateIdentityProviderClient(CognitoEnvironment environment)
    {
        var regionEndpoint = RegionEndpoint.GetBySystemName(environment.UserPoolId.Split('_')[0]);
        return new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(), regionEndpoint);
    }

    private static async Task<CognitoEnvironment> GetCognitoEnvironment()
    {
        try
        {
            // Lookup the cognito user-pool and client-id by taking a really good guess :crossed-fingers:.
            // This is not very pretty, but for now it does the trick.

            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://app.timechimp.com/auth/prelogin");
            var body = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(body, @"src=""(?<label>/_next/static/chunks/pages/_app-[0-9a-f]{16}.js)""");
            var script = match.Groups["label"].Value;
            response = await httpClient.GetAsync($"https://app.timechimp.com{script}");
            body = await response.Content.ReadAsStringAsync();
            match = Regex.Match(body, @"userPoolId:\s*""(?<pool>[^""]+)""");
            var pool = match.Groups["pool"].Value;
            Console.WriteLine($"** guessing the aws cognito UserPoolId to be {pool}");
            match = Regex.Match(body, @"userPoolClientId:\s*""(?<client>[^""]+)""");
            var client = match.Groups["client"].Value;
            Console.WriteLine($"** guessing the aws cognito ClientId to be {client}");
            return new CognitoEnvironment(pool, client);
        }
        catch (Exception e)
        {
            throw new ApplicationException($"unable to guess the aws cognito environment: {e.Message}");
        }
    }

    public record CognitoEnvironment(string UserPoolId, string ClientId);

    public record AuthResult(string AccessToken, DateTimeOffset Expires, string RefreshToken, CognitoEnvironment Environment)
    {
        public static AuthResult FromAuthenticationResultType(AuthenticationResultType auth, CognitoEnvironment environment)
        {
            return new AuthResult(auth.AccessToken, DateTimeOffset.UtcNow.AddSeconds(auth.ExpiresIn), auth.RefreshToken, environment);
        }
    }
}
