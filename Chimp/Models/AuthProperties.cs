namespace Chimp.Models;

public class AuthProperties
{
    public required string LoginUserName { get; init; }
    public string? LoginPassword { get; init; }
    public required string CognitoUserPoolId { get; init; }
    public required string CognitoClientId { get; init; }
    public required string AccessToken { get; set; }
    public required DateTimeOffset Expires { get; set; }
    public required string RefreshToken { get; set; }
}