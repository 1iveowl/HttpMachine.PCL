using System;
using System.Collections.Generic;
using System.Text;

namespace HeaderMatcher.Benchmarks;

/// <summary>The HTTP messages the end-to-end parser benchmarks replay.</summary>
internal static class Messages
{
    /// <summary>A browser-style request. Canonical casing, as browsers and HttpClient send it.</summary>
    private const string Request =
        "GET /path/to/resource?query=value HTTP/1.1\r\n" +
        "Host: example.com\r\n" +
        "User-Agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0\r\n" +
        "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n" +
        "Accept-Language: en-GB,en;q=0.9\r\n" +
        "Accept-Encoding: gzip, deflate, br\r\n" +
        "Connection: keep-alive\r\n" +
        "Cookie: session=abc123; theme=dark\r\n" +
        "Upgrade-Insecure-Requests: 1\r\n" +
        "Cache-Control: max-age=0\r\n" +
        "Content-Length: 0\r\n" +
        "\r\n";

    /// <summary>An nginx-style response. Canonical casing.</summary>
    private const string Response =
        "HTTP/1.1 200 OK\r\n" +
        "Server: nginx/1.24.0\r\n" +
        "Date: Tue, 28 Jul 2026 09:15:00 GMT\r\n" +
        "Content-Type: application/json; charset=utf-8\r\n" +
        "Content-Length: 26\r\n" +
        "Connection: keep-alive\r\n" +
        "Vary: Accept-Encoding\r\n" +
        "Cache-Control: no-cache, no-store\r\n" +
        "ETag: \"5f8a1c2e-1a\"\r\n" +
        "Last-Modified: Mon, 27 Jul 2026 12:00:00 GMT\r\n" +
        "Strict-Transport-Security: max-age=31536000\r\n" +
        "X-Request-Id: 7c9e6679-7425-40de-944b-e07fc1f90ae7\r\n" +
        "\r\n" +
        "{\"status\":\"ok\",\"id\":1234}\n";

    /// <summary>
    /// An SSDP NOTIFY as UPnP devices actually send it: upper-case field names throughout.
    /// </summary>
    /// <remarks>
    /// This message decides whether the default mode helps SSDP consumers. Note the split it
    /// creates: ST, USN, NT, NTS and BOOTID.UPNP.ORG are canonically upper case and hit the cached
    /// instance, while HOST, LOCATION, CACHE-CONTROL and SERVER are canonically Train-Case and so
    /// must be materialised unless HeaderNameMode.Canonical is selected. Writing this message in
    /// whichever casing flattered the result would make the whole benchmark worthless.
    /// </remarks>
    private const string SsdpNotify =
        "NOTIFY * HTTP/1.1\r\n" +
        "HOST: 239.255.255.250:1900\r\n" +
        "CACHE-CONTROL: max-age=1800\r\n" +
        "LOCATION: http://192.168.1.10:80/description.xml\r\n" +
        "NT: upnp:rootdevice\r\n" +
        "NTS: ssdp:alive\r\n" +
        "SERVER: Linux/5.10 UPnP/2.0 Example/1.0\r\n" +
        "USN: uuid:9f7c1a2b-3d4e::upnp:rootdevice\r\n" +
        "BOOTID.UPNP.ORG: 1\r\n" +
        "CONFIGID.UPNP.ORG: 1337\r\n" +
        "\r\n";

    private static readonly Dictionary<string, byte[]> All = new(StringComparer.Ordinal)
    {
        ["Request"] = Encoding.UTF8.GetBytes(Request),
        ["Response"] = Encoding.UTF8.GetBytes(Response),
        ["SsdpNotify"] = Encoding.UTF8.GetBytes(SsdpNotify),
    };

    public static byte[] Get(string name) => All[name];
}
