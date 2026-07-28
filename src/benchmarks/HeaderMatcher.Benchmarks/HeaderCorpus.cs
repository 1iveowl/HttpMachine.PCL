using System.Text;

namespace HeaderMatcher.Benchmarks;

/// <summary>
/// The header names the matcher benchmarks iterate over.
/// </summary>
/// <remarks>
/// Chosen to be representative rather than flattering. It mixes known headers across the whole
/// length spectrum, deliberately over-weights the crowded 13- and 16-byte buckets where several
/// known names share a length, and includes unknown names and near-misses that share a length with
/// a known header but differ in one byte. A corpus of nothing but canonical known headers would
/// measure the best case and tell us very little.
/// </remarks>
internal static class HeaderCorpus
{
    /// <summary>Must equal <see cref="Names"/>.Length; see the assertion in the benchmark setup.</summary>
    public const int Size = 48;

    public static readonly string[] Names =
    [
        // ---- Canonical casing, as browsers, curl, nginx and HttpClient send them ----
        "Host",
        "User-Agent",
        "Accept",
        "Accept-Encoding",
        "Accept-Language",
        "Connection",
        "Content-Length",
        "Content-Type",
        "Cache-Control",
        "Cookie",
        "Date",
        "Server",
        "Set-Cookie",
        "Last-Modified",
        "ETag",
        "Transfer-Encoding",
        "Content-Encoding",
        "Vary",
        "Age",
        "Via",
        "TE",
        "Strict-Transport-Security",
        "If-Modified-Since",
        "Authorization",

        // ---- Length-collision pressure: the 13- and 16-byte buckets hold six names each ----
        "Accept-Ranges",
        "Content-Range",
        "If-None-Match",
        "Content-Language",
        "Content-Location",
        "WWW-Authenticate",

        // ---- Non-canonical casing: known headers that miss the exact-casing fast path ----
        "CONTENT-LENGTH",
        "content-type",
        "Keep-alive",
        "USER-AGENT",

        // ---- SSDP / UPnP, in the upper case UPnP devices actually send ----
        "ST",
        "USN",
        "NTS",
        "EXT",
        "MAN",
        "BOOTID.UPNP.ORG",

        // ---- Unknown ----
        "X-Request-Id",
        "X-Correlation-Id",
        "A",
        "X-Amz-Cf-Id",

        // ---- Near misses: same length as a known header, one byte different ----
        "Content-Lengtx",
        "Cache-Contral",
        "Conten-Length",
        "Hosx",
    ];

    /// <summary>The same names as UTF-8, which is what the parser actually holds.</summary>
    public static readonly byte[][] Utf8 = BuildUtf8();

    private static byte[][] BuildUtf8()
    {
        var result = new byte[Names.Length][];

        for (var i = 0; i < Names.Length; i++)
        {
            result[i] = Encoding.UTF8.GetBytes(Names[i]);
        }

        return result;
    }
}
