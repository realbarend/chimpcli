namespace Chimp.Api.AwsCognito;

internal record CognitoTokens(string AccessToken, int ExpiresIn, string? RefreshToken);
