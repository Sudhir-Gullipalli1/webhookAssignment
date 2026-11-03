using System.Security.Cryptography;
using System.Text;

namespace WebhookReceiver;

public static class StripeSignatureVerifier
{
    private const int ToleranceSeconds = 300;

    public static bool Verify(string payload, string signatureHeader, string secret)
    {
        if (string.IsNullOrEmpty(signatureHeader))
            return false;

        string? timestamp = null;
        var signatures = new List<string>();

        foreach (var item in signatureHeader.Split(','))
        {
            var parts = item.Split('=', 2);
            if (parts.Length != 2)
                continue;

            if (parts[0] == "t")
                timestamp = parts[1];
            else if (parts[0] == "v1")
                signatures.Add(parts[1]);
        }

        if (!long.TryParse(timestamp, out var t) || signatures.Count == 0)
            return false;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - t) > ToleranceSeconds)
            return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{payload}"));
        var expected = Encoding.UTF8.GetBytes(Convert.ToHexString(hash).ToLower());

        return signatures.Any(s => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(s), expected));
    }
}
