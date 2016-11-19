using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HttpMachine.NETCore.UnitTest.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpMachine.NETCore.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        private IDictionary<string, string> _headers;

        [TestMethod]
        public void TestReponseWithBodyUsingContentLength()
        {
            //Arrange
            using (var handler = new HttpParserDelegate(numberOfChunks: 0))
            using (var parser = new HttpCombinedParser(handler))
            {
                _headers = new Dictionary<string, string>()
                {
                    {"CONTENT-TYPE", "text/plain" },
                    {"CACHE-CONTROL", "max-age = 10"}
                };

                var content = "This is a test";

                var responseDatagram = TestReponseComposer(_headers, content);

                //Act
                var bytesParsed = parser.Execute(new ArraySegment<byte>(responseDatagram, 0, responseDatagram.Length));

                var isParserSuccessful = bytesParsed == responseDatagram.Length;

                handler.HttpRequestReponse.Body.Position = 0;
                var reader = new StreamReader(handler.HttpRequestReponse.Body);
                var responseContent = reader.ReadToEnd();
                var responseHeaders = handler.HttpRequestReponse.Headers;

                IEqualityComparer<string> valueComparer = EqualityComparer<string>.Default;

                //Assert
                Assert.IsTrue(isParserSuccessful);
                Assert.AreEqual(content, responseContent);
                Assert.IsTrue(_headers.Count == responseHeaders.Count
                    && _headers.Keys.All(key => responseHeaders.ContainsKey(key)
                    && valueComparer.Equals(_headers[key], responseHeaders[key])));
            }
        }

        [TestMethod]
        public void TestReponseWithChunkedBody()
        {
            var chunks = new List<string>
            {
                "test",
                "This is a longer text string to test how the code copes with multi digit hex length. This string is longer than 16.",
                "The end is near",
                "The end is here... \r\n almost on this second line",
                "Stop"
            };

            var content = string.Join("", chunks);

            //Arrange
            using (var handler = new HttpParserDelegate(chunks.Count))
            using (var parser = new HttpCombinedParser(handler))
            {
                _headers = new Dictionary<string, string>()
                {
                    {"CONTENT-TYPE", "text/plain" },
                    {"CACHE-CONTROL", "max-age = 10"},
                };


                var responseDatagram = TestResponseChunkedComposer(_headers, chunks);

                //Act
                var bytesParsed = parser.Execute(new ArraySegment<byte>(responseDatagram, 0, responseDatagram.Length));

                var isParserSuccessful = bytesParsed == responseDatagram.Length;

                handler.HttpRequestReponse.Body.Position = 0;
                var reader = new StreamReader(handler.HttpRequestReponse.Body);
                var responseContent = reader.ReadToEnd();
                var responseHeaders = handler.HttpRequestReponse.Headers;

                IEqualityComparer<string> valueComparer = EqualityComparer<string>.Default;

                //Assert
                Assert.IsTrue(isParserSuccessful);
                Assert.AreEqual(content, responseContent);

                Assert.IsTrue(_headers.Count == responseHeaders.Count
                    && _headers.Keys.All(key => responseHeaders.ContainsKey(key)
                    && valueComparer.Equals(_headers[key], responseHeaders[key])));
            }
        }

        private byte[] TestReponseComposer(IDictionary<string, string> headerDictionary, string content)
        {
            var stringBuilder = HeaderStringBuilder(headerDictionary);

            return ComposeDatagramWithBody(stringBuilder, content);
        }

        private byte[] TestResponseChunkedComposer(IDictionary<string, string> headerDictionary, IEnumerable<string> chunkList)
        {
            var headerStringBuilder = HeaderStringBuilder(headerDictionary);
            var chunksStringBuilder = new StringBuilder();

            foreach (var part in chunkList)
            {
                if (part.Length > 0)
                {
                    //var headersStringBuilder = new StringBuilder();
                    chunksStringBuilder.Append(part.Length.ToString("X")); // Convert length to hex string
                    chunksStringBuilder.Append("\r\n");
                    chunksStringBuilder.Append(part);
                    chunksStringBuilder.Append("\r\n"); //Trailing crlf - not counted towards the length.
                }
            }
            chunksStringBuilder.Append($"{0:X}\r\n\r\n"); // End chunking

            return ComposeDatagramWithBody(headerStringBuilder, chunksStringBuilder.ToString(), isChunked: true);
        }

        private StringBuilder HeaderStringBuilder(IDictionary<string, string> headerDictionary)
        {
            var headerStringBuilder = new StringBuilder();

            headerStringBuilder.Append("HTTP/1.1 200 OK\r\n");

            foreach (var header in headerDictionary)
            {
                headerStringBuilder.Append($"{header.Key}: {header.Value}\r\n");
            }
            return headerStringBuilder;
        }

        private byte[] ComposeDatagramWithBody(StringBuilder headersStringBuilder, string content, bool isChunked = false)
        {
            byte[] datagram = null;

            if (content.Length > 0)
            {
                if (!isChunked) // Add Content-Length when message is not chunked.
                {

                    _headers.Add("CONTENT-LENGTH", content.Length.ToString());

                    headersStringBuilder.Append($"CONTENT-LENGTH: {content.Length}");
                }
                else
                {
                    _headers.Add("TRANSFER-ENCODING", "chunked");
                    headersStringBuilder.Append("Transfer-Encoding: chunked");

                }
            }

            headersStringBuilder.Append($"\r\n\r\n");
            datagram = Encoding.UTF8.GetBytes(headersStringBuilder.ToString());

            if (content?.Length > 0)
            {
                datagram = Encoding.UTF8.GetBytes(headersStringBuilder.ToString());
                datagram = datagram?.Concat(Encoding.UTF8.GetBytes(content)).ToArray();
            }

            return datagram;
        }
    }
}
