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
            using (var handler = new HttpParserDelegate(numberOfChunks:0))
            using (var parser = new HttpCombinedParser(handler))
            {
                _headers = new Dictionary<string, string>()
                {
                    {"Content-Type".ToUpper(), "text/plain" },
                    {"CACHE-CONTROL".ToUpper(), "max-age = 10"}
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

        private byte[] TestReponseComposer(IDictionary<string, string> headerDictionary, string content)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("HTTP/1.1 200 OK\r\n");

            foreach (var header in headerDictionary)
            {
                stringBuilder.Append($"{header.Key}: {header.Value}\r\n");
            }

            return ComposeDatagramWithBody(stringBuilder, content);
        }

        private byte[] ComposeDatagramWithBody(StringBuilder stringBuilder, string body, bool isChunked = false)
        {
            byte[] datagram = null;

            if (!isChunked) // Add Content-Length with message is not chunked.
            {
                if (body.Length > 0)
                {
                    _headers.Add("Content-Length".ToUpper(), body.Length.ToString()); // 

                    stringBuilder.Append($"Content-Length: {body.Length}");
                    stringBuilder.Append($"\r\n\r\n");
                    datagram = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                }
                else
                {
                    stringBuilder.Append($"\r\n\r\n");
                }

            }

            if (body?.Length > 0)
            {
                datagram = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                datagram = datagram?.Concat(Encoding.UTF8.GetBytes(body)).ToArray();
            }

            return datagram;
        }
    }
}
