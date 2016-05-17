﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpMachine.Tests
{
    class TestRequest
    {
        public string Name;
        public byte[] Raw;
        public string Method;
        public string RequestUri;
        public string RequestPath;
        public string QueryString;
        public string Fragment;
        public int VersionMajor;
        public int VersionMinor;
        public Dictionary<string, string> Headers;
        public byte[] Body;

        public bool ShouldKeepAlive; // if the message is 1.1 and !Connection:close, or message is < 1.1 and Connection:keep-alive
        public bool OnHeadersEndCalled;

        public int? StatusCode;
        public string StatusReason;

        // based on this variable we will create needed ParserDelegate
        public bool IsRequest = true;

        public static TestRequest[] Requests = new TestRequest[] {

            new TestRequest() {
                Name = "No headers, no body",
                Raw = Encoding.UTF8.GetBytes("\r\nGET /foo HTTP/1.1\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>() {
                },
                Body = null,
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "No headers, no body, no version",
                Raw = Encoding.UTF8.GetBytes("GET /foo\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 0,
                VersionMinor = 9,
                Headers = new Dictionary<string,string>() {
                },
                Body = null,
                ShouldKeepAlive = false
            },
            new TestRequest() {
                Name = "no body",
                Raw = Encoding.UTF8.GetBytes("GET /foo HTTP/1.1\r\nFoo: Bar\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>() {
                    { "Foo", "Bar" }
                },
                Body = null,
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "no body no version",
                Raw = Encoding.UTF8.GetBytes("GET /foo\r\nFoo: Bar\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 0,
                VersionMinor = 9,
                Headers = new Dictionary<string,string>() {
                    { "Foo", "Bar" }
                },
                Body = null,
                ShouldKeepAlive = false
            },
            new TestRequest() {
                Name = "query string",
                Raw = Encoding.UTF8.GetBytes("GET /foo?asdf=jklol HTTP/1.1\r\nFoo: Bar\r\nBaz-arse: Quux\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo?asdf=jklol",
                RequestPath = "/foo",
                QueryString = "asdf=jklol",
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>() {
                    { "Foo", "Bar" },
                    { "Baz-arse", "Quux" }
                },
                Body = null,
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "fragment",
                Raw = Encoding.UTF8.GetBytes("POST /foo?asdf=jklol#poopz HTTP/1.1\r\nFoo: Bar\r\nBaz: Quux\r\n\r\n"),
                Method = "POST",
                RequestUri = "/foo?asdf=jklol#poopz",
                RequestPath = "/foo",
                QueryString = "asdf=jklol",
                Fragment = "poopz",
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>() {
                    { "Foo", "Bar" },
                    { "Baz", "Quux" }
                },
                Body = null,
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "digits in path",
                Raw = Encoding.UTF8.GetBytes("GET /foo/500.html HTTP/1.1\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo/500.html",
                RequestPath = "/foo/500.html",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>() {
                },
                Body = null,
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "digits in query string",
                Raw = Encoding.UTF8.GetBytes("GET /foo?123=abc&def=567 HTTP/1.1\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo?123=abc&def=567",
                RequestPath = "/foo",
                QueryString = "123=abc&def=567",
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>() {
                },
                Body = null,
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "digits in path and query string",
                Raw = Encoding.UTF8.GetBytes("GET /foo/500.html?123=abc&def=567 HTTP/1.1\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo/500.html?123=abc&def=567",
                RequestPath = "/foo/500.html",
                QueryString = "123=abc&def=567",
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>() {
                },
                Body = null,
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "zero content length",
                Raw = Encoding.UTF8.GetBytes("POST /foo HTTP/1.1\r\nFoo: Bar\r\nContent-Length: 0\r\n\r\n"),
                Method = "POST",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Foo", "Bar" },
                    { "Content-Length", "0" }
                },
                Body = null,
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "some content length",
                Raw = Encoding.UTF8.GetBytes("POST /foo HTTP/1.1\r\nFoo: Bar\r\nContent-Length: 5\r\n\r\nhello"),
                Method = "POST",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Foo", "Bar" },
                    { "Content-Length", "5" }
                },
                Body = Encoding.UTF8.GetBytes("hello"),
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "1.1 get",
                Raw = Encoding.UTF8.GetBytes("GET /foo HTTP/1.1\r\nFoo: Bar\r\nConnection: keep-alive\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Foo", "Bar" },
                    { "Connection", "keep-alive" }
                },
                Body = null,
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "1.1 get close",
                Raw = Encoding.UTF8.GetBytes("GET /foo HTTP/1.1\r\nFoo: Bar\r\nConnection: CLOSE\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Foo", "Bar" },
                    { "CoNNection", "CLOSE" }
                },
                Body = null,
                ShouldKeepAlive = false
            },
            new TestRequest() {
                Name = "1.1 post",
                Raw = Encoding.UTF8.GetBytes("POST /foo HTTP/1.1\r\nFoo: Bar\r\nContent-Length: 15\r\n\r\nhelloworldhello"),
                Method = "POST",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Foo", "Bar" },
                    { "Content-Length", "15" }
                },
                Body = Encoding.UTF8.GetBytes("helloworldhello"),
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "1.1 post close",
                Raw = Encoding.UTF8.GetBytes("POST /foo HTTP/1.1\r\nFoo: Bar\r\nContent-Length: 15\r\nConnection: close\r\nBaz: Quux\r\n\r\nhelloworldhello"),
                Method = "POST",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Foo", "Bar" },
                    { "Content-Length", "15" },
                    { "Connection", "close" },
                    { "Baz", "Quux" }
                },
                Body = Encoding.UTF8.GetBytes("helloworldhello"),
                ShouldKeepAlive = false
            },
            // because it has no content-length, it's not keep alive anyway? TODO 
            new TestRequest() {
                Name = "get connection close",
                Raw = Encoding.UTF8.GetBytes("GET /foo?asdf=jklol#poopz HTTP/1.1\r\nFoo: Bar\r\nBaz: Quux\r\nConnection: close\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo?asdf=jklol#poopz",
                RequestPath = "/foo",
                QueryString = "asdf=jklol",
                Fragment = "poopz",
                VersionMajor = 1,
                VersionMinor = 1,
                Headers = new Dictionary<string,string>() {
                    { "Foo", "Bar" },
                    { "Baz", "Quux" },
                    { "Connection", "close" }
                },
                Body = null,
                ShouldKeepAlive = false
            },
            new TestRequest() {
                Name = "1.0 get",
                Raw = Encoding.UTF8.GetBytes("GET /foo?asdf=jklol#poopz HTTP/1.0\r\nFoo: Bar\r\nBaz: Quux\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo?asdf=jklol#poopz",
                RequestPath = "/foo",
                QueryString = "asdf=jklol",
                Fragment = "poopz",
                VersionMajor = 1,
                VersionMinor = 0,
                Headers = new Dictionary<string,string>() {
                    { "Foo", "Bar" },
                    { "Baz", "Quux" }
                },
                Body = null,
                ShouldKeepAlive = false
            },
            new TestRequest() {
                Name = "1.0 get keep-alive",
                Raw = Encoding.UTF8.GetBytes("GET /foo?asdf=jklol#poopz HTTP/1.0\r\nFoo: Bar\r\nBaz: Quux\r\nConnection: keep-alive\r\n\r\n"),
                Method = "GET",
                RequestUri = "/foo?asdf=jklol#poopz",
                RequestPath = "/foo",
                QueryString = "asdf=jklol",
                Fragment = "poopz",
                VersionMajor = 1,
                VersionMinor = 0,
                Headers = new Dictionary<string,string>() {
                    { "Foo", "Bar" },
                    { "Baz", "Quux" },
                    { "Connection", "keep-alive" }
                },
                Body = null,
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "1.0 post",
                Raw = Encoding.UTF8.GetBytes("POST /foo HTTP/1.0\r\nContent-Length: 15\r\nFoo: Bar\r\n\r\nhelloworldhello"),
                Method = "POST",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 0,
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Content-Length", "15" },
                    { "Foo", "Bar" }
                },
                Body = Encoding.UTF8.GetBytes("helloworldhello"),
                ShouldKeepAlive = false
            },
            new TestRequest() {
                Name = "1.0 post no content length",
                Raw = Encoding.UTF8.GetBytes("POST /foo HTTP/1.0\r\nFoo: Bar\r\n\r\nhelloworldhello"),
                Method = "POST",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 0,
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Foo", "Bar" }
                },
                Body = null,
                ShouldKeepAlive = false
            },
            new TestRequest() {
                Name = "1.0 post keep-alive with content length",
                Raw = Encoding.UTF8.GetBytes("POST /foo HTTP/1.0\r\nContent-Length: 15\r\nFoo: Bar\r\nConnection: keep-alive\r\n\r\nhelloworldhello"),
                Method = "POST",
                RequestUri = "/foo",
                RequestPath = "/foo",
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 0,
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Foo", "Bar" },
                    { "Connection", "keep-alive" },
                    { "Content-Length", "15" }
                },
                Body = Encoding.UTF8.GetBytes("helloworldhello"),
                ShouldKeepAlive = true
            },
            new TestRequest() {
                Name = "Response 1.0 simple",
                Raw = Encoding.UTF8.GetBytes("HTTP/1.0 200 OK\r\n\r\n"),
                Method = null,
                RequestUri = null,
                RequestPath = null,
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 0,
                StatusCode = 200,
                StatusReason = "OK",
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase),
                Body = null,
                ShouldKeepAlive = false,
                IsRequest = false
            },
            new TestRequest() {
                Name = "Response 1.1 simple",
                Raw = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 3\r\nConnection: keep-alive\r\n\r\n123"),
                Method = null,
                RequestUri = null,
                RequestPath = null,
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Body = Encoding.UTF8.GetBytes("123"),
                StatusCode = 200,
                StatusReason = "OK",
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Content-Length", "3" },
                    { "Connection", "keep-alive"}
                },
                ShouldKeepAlive = true,
                IsRequest = false
            },
            new TestRequest() {
                Name = "Response 1.1 headers",
                Raw = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 15\r\nFoo: Bar\r\nConnection: keep-alive\r\n\r\nhelloworldhello"),
                Method = null,
                RequestUri = null,
                RequestPath = null,
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Body = Encoding.UTF8.GetBytes("helloworldhello"),
                StatusCode = 200,
                StatusReason = "OK",
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Foo", "Bar" },
                    { "Connection", "keep-alive" },
                    { "Content-Length", "15" }
                },
                ShouldKeepAlive = true,
                IsRequest = false
            },
            new TestRequest() {
                Name = "Response 1.1 redirect",
                Raw = Encoding.UTF8.GetBytes("HTTP/1.1 302 Found\r\nLocation: http://www.iana.org/domains/example/\r\n\r\n"),
                Method = null,
                RequestUri = null,
                RequestPath = null,
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Body = null,
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Location", "http://www.iana.org/domains/example/" }
                },
                StatusCode = 302,
                StatusReason = "Found",
                ShouldKeepAlive = true,
                IsRequest = false
            },
            new TestRequest() {
                Name = "Response 1.1 redirect body",
                Raw = Encoding.UTF8.GetBytes("HTTP/1.1 302 Found\r\nLocation: http://www.iana.org/domains/example/\r\nContent-Type: text/html\r\nContent-Length: 19\r\n\r\nThis page has moved"),
                Method = null,
                RequestUri = null,
                RequestPath = null,
                QueryString = null,
                Fragment = null,
                VersionMajor = 1,
                VersionMinor = 1,
                Body = Encoding.UTF8.GetBytes("This page has moved"),
                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
                    { "Location", "http://www.iana.org/domains/example/" },
                    { "Content-Type", "text/html"},
                    {"Content-Length", "19"}
                },
                StatusCode = 302,
                StatusReason = "Found",
                ShouldKeepAlive = true,
                IsRequest = false
            },

//
//            // i know you're not supposed to comment out tests, but this just takes to long to run
//
//            new TestRequest() {
//                Name = "safari",
//                Raw = Encoding.UTF8.GetBytes(@"GET /portfolio HTTP/1.1
//Host: bvanderveen.com
//User-Agent: Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_5; en-us) AppleWebKit/533.18.1 (KHTML, like Gecko) Version/5.0.2 Safari/533.18.5
//Accept: application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
//Referer: http://bvanderveen.com/
//Accept-Language: en-us
//Accept-Encoding: gzip, deflate
//Cookie:  __utma=7373..111.99; __utmz=.utmcsr=google|utmccn=(organic)|utmcmd=organic|utmctr=fooobnn%20ittszz
//Connection: keep-alive
//
//"),
//                Method = "GET",
//                RequestUri = "/portfolio",
//                RequestPath = "/portfolio",
//                QueryString = null,
//                Fragment = null,
//                VersionMajor = 1,
//                VersionMinor = 1,
//                Headers = new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {
//                    { "Host", "bvanderveen.com" },
//                    { "User-Agent", "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_5; en-us) AppleWebKit/533.18.1 (KHTML, like Gecko) Version/5.0.2 Safari/533.18.5" },
//                    { "Accept", "application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5" },
//                    { "Referer", "http://bvanderveen.com/" },
//                    { "Accept-Language", "en-us" },
//                    { "Accept-Encoding", "gzip, deflate" },
//                    { "Cookie", "__utma=7373..111.99; __utmz=.utmcsr=google|utmccn=(organic)|utmcmd=organic|utmctr=fooobnn%20ittszz" },
//                    { "Connection", "keep-alive" }
//                },
//                Body = null,
//                ShouldKeepAlive = true
//            },

        };
    }
}
