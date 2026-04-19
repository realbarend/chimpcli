using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Chimp.Api.AwsCognito;

internal static class CognitoSrp
{
    // 3072-bit prime N used by AWS Cognito (RFC 5054 group 18)
    private static readonly BigInteger N = BigInteger.Parse(
        "00FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD1" +
        "29024E088A67CC74020BBEA63B139B22514A08798E3404DD" +
        "EF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245" +
        "E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7ED" +
        "EE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3D" +
        "C2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F" +
        "83655D23DCA3AD961C62F356208552BB9ED529077096966D" +
        "670C354E4ABC9804F1746C08CA18217C32905E462E36CE3B" +
        "E39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9" +
        "DE2BCBF6955817183995497CEA956AE515D2261898FA0510" +
        "15728E5A8AAAC42DAD33170D04507A33A85521ABDF1CBA64" +
        "ECFB850458DBEF0A8AEA71575D060C7DB3970F85A6E1E4C7" +
        "ABF5AE8CDB0933D71E8C94E04A25619DCEE3D2261AD2EE6B" +
        "F12FFA06D98A0864D87602733EC86A64521F2B18177B200C" +
        "BBE117577A615D6C770988C0BAD946E208E24FA074E5AB31" +
        "43DB5BFCE0FD108E4B82D120A93AD2CAFFFFFFFFFFFFFFFF",
        System.Globalization.NumberStyles.HexNumber);

    private static readonly BigInteger G = 2;

    // k = H(signedPad(N) || signedPad(g)) — computed once at startup
    private static readonly BigInteger K =
        new BigInteger(SHA256.HashData(Concat(SignedPad(N), SignedPad(G))), isUnsigned: true, isBigEndian: true);

    public static (BigInteger a, string srpAHex) GenerateSrpA()
    {
        var a = new BigInteger(RandomNumberGenerator.GetBytes(128), isUnsigned: true);
        var A = BigInteger.ModPow(G, a, N);
        return (a, Convert.ToHexString(A.ToByteArray(isUnsigned: true, isBigEndian: true)));
    }

    public static (string signature, string timestamp) ComputePasswordClaim(
        string poolName, string userIdForSrp, string password,
        BigInteger a, string srpAHex, string srpBHex, string saltHex, string secretBlockBase64)
    {
        var A = BigInteger.Parse("00" + srpAHex, System.Globalization.NumberStyles.HexNumber);
        var B = BigInteger.Parse("00" + srpBHex, System.Globalization.NumberStyles.HexNumber);
        var salt = BigInteger.Parse("00" + saltHex, System.Globalization.NumberStyles.HexNumber);
        var secretBlock = Convert.FromBase64String(secretBlockBase64);

        // u = H(signedPad(A) || signedPad(B))
        var uBytes = SHA256.HashData(Concat(SignedPad(A), SignedPad(B)));
        var u = new BigInteger(uBytes, isUnsigned: true, isBigEndian: true);

        // x = H(signedPad(salt) || H(poolName + userIdForSrp + ":" + password))
        var innerHash = SHA256.HashData(Encoding.UTF8.GetBytes(poolName + userIdForSrp + ":" + password));
        var x = new BigInteger(SHA256.HashData(Concat(SignedPad(salt), innerHash)), isUnsigned: true, isBigEndian: true);

        // S = (B - k*g^x mod N) ^ (a + u*x) mod N
        var kgx = K * BigInteger.ModPow(G, x, N) % N;
        var S = BigInteger.ModPow(((B - kgx) % N + N) % N, a + u * x, N);

        var hkdfKey = HkdfCognito(SignedPad(S), SignedPad(u));

        // Cognito timestamp format: "ddd MMM d HH:mm:ss UTC yyyy" (day without zero-padding)
        var timestamp = DateTime.UtcNow.ToString("ddd MMM d HH:mm:ss 'UTC' yyyy", System.Globalization.CultureInfo.InvariantCulture);

        var msg = Concat(Encoding.UTF8.GetBytes(poolName), Encoding.UTF8.GetBytes(userIdForSrp), secretBlock, Encoding.UTF8.GetBytes(timestamp));
        return (Convert.ToBase64String(HMACSHA256.HashData(hkdfKey, msg)), timestamp);
    }

    // HKDF-SHA256 matching Amazon.Extensions.CognitoAuthentication:
    // Extract: PRK = HMAC-SHA256(key=uBytes, data=sBytes)
    // Expand:  T1  = HMAC-SHA256(key=PRK, data=info + 0x01)  (empty hashedBlock on first iteration)
    private static byte[] HkdfCognito(byte[] sBytes, byte[] uBytes)
    {
        var prk = HMACSHA256.HashData(key: uBytes, source: sBytes);
        var expandInput = Concat(Encoding.UTF8.GetBytes("Caldera Derived Key"), [0x01]);
        return HMACSHA256.HashData(key: prk, source: expandInput)[..16];
    }

    // Signed big-endian: prepend 0x00 if the high bit is set.
    // Matches Java BigInteger.toByteArray() and JS/Python padHex() used by all Cognito SDK reference implementations.
    private static byte[] SignedPad(BigInteger value)
    {
        var bytes = value.ToByteArray(isUnsigned: true, isBigEndian: true);
        return (bytes[0] & 0x80) != 0 ? [0x00, .. bytes] : bytes;
    }

    private static byte[] Concat(params byte[][] arrays)
    {
        var result = new byte[arrays.Sum(a => a.Length)];
        var offset = 0;
        foreach (var a in arrays) { a.CopyTo(result, offset); offset += a.Length; }
        return result;
    }
}
