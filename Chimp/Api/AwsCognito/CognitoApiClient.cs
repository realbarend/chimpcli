using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Chimp.Api.AwsCognito;

/// <summary>
/// This is a very thin wrapper around the AWS Cognito API.
/// It is introduced to not depend on the AWS SDK.
/// Main reason is to allow us to use trimmed binaries.
/// </summary>
internal sealed class CognitoApiClient(HttpClient httpClient, string userPoolId, string clientId)
{
    private readonly string _poolName = userPoolId.Split('_')[1];
    private readonly Uri _endpoint = new($"https://cognito-idp.{userPoolId.Split('_')[0]}.amazonaws.com/");

    public record MfaChallenge(string UserIdForSrp, string Session);

    internal sealed class AuthenticationResult(CognitoTokens? tokens, MfaChallenge? mfaChallenge)
    {
        [MemberNotNullWhen(false, nameof(Tokens))]
        [MemberNotNullWhen(true, nameof(MfaChallenge))]
        public bool RequiresMfa => mfaChallenge != null;

        public CognitoTokens? Tokens => tokens;
        public MfaChallenge? MfaChallenge => mfaChallenge;
    }

    // Runs the full SRP handshake. Returns an AuthenticationResult: check RequiresMfa to determine next step.
    internal async Task<AuthenticationResult> AuthenticateSrp(string userName, string password)
    {
        var (a, srpAHex) = CognitoSrp.GenerateSrpA();

        var srp = await PostAsync("InitiateAuth", new JsonObject {
            ["AuthFlow"] = "USER_SRP_AUTH",
            ["ClientId"] = clientId,
            ["AuthParameters"] = new JsonObject { ["USERNAME"] = userName, ["SRP_A"] = srpAHex }
        });

        var cp = srp["ChallengeParameters"]!.AsObject();
        var userIdForSrp = cp["USER_ID_FOR_SRP"]!.GetValue<string>();
        var (signature, timestamp) = CognitoSrp.ComputePasswordClaim(
            _poolName, userIdForSrp, password, a, srpAHex,
            cp["SRP_B"]!.GetValue<string>(),
            cp["SALT"]!.GetValue<string>(),
            cp["SECRET_BLOCK"]!.GetValue<string>());

        var auth = await PostAsync("RespondToAuthChallenge", new JsonObject {
            ["ChallengeName"] = "PASSWORD_VERIFIER",
            ["ClientId"] = clientId,
            ["ChallengeResponses"] = new JsonObject {
                ["USERNAME"] = userIdForSrp,
                ["PASSWORD_CLAIM_SIGNATURE"] = signature,
                ["PASSWORD_CLAIM_SECRET_BLOCK"] = cp["SECRET_BLOCK"]!.GetValue<string>(),
                ["TIMESTAMP"] = timestamp
            }
        });

        if (auth["ChallengeName"]?.GetValue<string>() == "SOFTWARE_TOKEN_MFA")
            return new AuthenticationResult(null, new MfaChallenge(userIdForSrp, auth["Session"]!.GetValue<string>()));

        return new AuthenticationResult(ParseTokens(auth), null);
    }

    internal async Task<CognitoTokens> CompleteMfaChallenge(MfaChallenge challenge, string code)
    {
        var response = await PostAsync("RespondToAuthChallenge", new JsonObject {
            ["ChallengeName"] = "SOFTWARE_TOKEN_MFA",
            ["ClientId"] = clientId,
            ["Session"] = challenge.Session,
            ["ChallengeResponses"] = new JsonObject {
                ["USERNAME"] = challenge.UserIdForSrp,
                ["SOFTWARE_TOKEN_MFA_CODE"] = code
            }
        });
        return ParseTokens(response);
    }

    internal async Task<CognitoTokens> RefreshToken(string refreshToken)
    {
        var response = await PostAsync("InitiateAuth", new JsonObject {
            ["AuthFlow"] = "REFRESH_TOKEN_AUTH",
            ["ClientId"] = clientId,
            ["AuthParameters"] = new JsonObject { ["REFRESH_TOKEN"] = refreshToken }
        });
        return ParseTokens(response);
    }

    private static CognitoTokens ParseTokens(JsonObject response)
    {
        var r = response["AuthenticationResult"]!.AsObject();
        return new CognitoTokens(
            r["AccessToken"]!.GetValue<string>(),
            r["ExpiresIn"]!.GetValue<int>(),
            r["RefreshToken"]?.GetValue<string>());
    }

    private async Task<JsonObject> PostAsync(string action, JsonObject body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        request.Headers.Add("X-Amz-Target", $"AWSCognitoIdentityProviderService.{action}");
        request.Content = new StringContent(body.ToJsonString(), System.Text.Encoding.UTF8, "application/x-amz-json-1.1");

        using var response = await httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        var node = JsonNode.Parse(json)!.AsObject();

        if (response.IsSuccessStatusCode) return node;

        var msg = node["message"]?.GetValue<string>() ?? node["Message"]?.GetValue<string>() ?? response.ReasonPhrase;

        throw new Exception("login failed: " + (msg ?? "unknown error"));
    }
}
