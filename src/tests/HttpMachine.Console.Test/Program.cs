using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMachine.Console.Test.Model;

namespace HttpMachine.Console.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var handler = new ParserHandler();

            var parser = new HttpCombinedParser(handler);

            byte[] bArray;

            bArray = TestReponse();
            System.Console.WriteLine(parser.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) == bArray.Length
                ? $"Reponse test succeed. Type identified is; {handler.MessageType}"
                : $"Response test failed");

            handler.HttpRequestReponse.Body.Position = 0;
            var reader = new StreamReader(handler.HttpRequestReponse.Body);
            var body = reader.ReadToEnd();

            //bArray = Encoding.UTF8.GetBytes(TestRequest());
            //System.Console.WriteLine(parser.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) == bArray.Length
            //    ? $"Request test succeed. Type identified is; {handler.MessageType}"
            //    : $"Request test failed");


            parser = null;
            parser = new HttpCombinedParser(handler);

            bArray = TestChunkedResponse();
            System.Console.WriteLine(parser.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) == bArray.Length
                ? $"Chunked Response test succeed. Type identified is; {handler.MessageType}."
                : $"Chunked Response test failed");

            body = null;
            handler.HttpRequestReponse.Body.Position = 0;
            reader = new StreamReader(handler.HttpRequestReponse.Body);
            body = reader.ReadToEnd();

            System.Console.ReadKey();
        }

        private static byte[] TestReponse()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("HTTP/1.1 200 OK\r\n");
            stringBuilder.Append("CACHE-CONTROL: max-age = 10\r\n");
            stringBuilder.Append("ST: upnp:rootdevice\r\n");

            stringBuilder.Append("ST: upnp:rootdevice\r\n");
            stringBuilder.Append("USN: uuid:73796E6F-6473-6D00-0000-0011322fe5f0::upnp:rootdevice\r\n");
            stringBuilder.Append("EXT:\r\n");
            stringBuilder.Append("SERVER: Synology/DSM/192.168.0.33\r\n");
            stringBuilder.Append("LOCATION: http://192.168.0.33:5000/ssdp/desc-DSM-eth1.xml\r\n");
            stringBuilder.Append("OPT: \"http://schemas.upnp.org/upnp/1/0/\"; ns=01\r\n");
            stringBuilder.Append("01-NLS: 1\r\n");
            stringBuilder.Append("BOOTID.UPNP.ORG: 1\r\n");
            stringBuilder.Append("CONFIGID.UPNP.ORG: 1337\r\n");

            var body = "This is a test";

            return ComposeDatagramWithBody(stringBuilder, body);
        }

        private static byte[] TestChunkedResponse()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("HTTP/1.1 200 OK\r\n");
            stringBuilder.Append("Content-Type: test/plain\r\n");
            stringBuilder.Append("Transfer-Encoding: chunked\r\n" +
                                 "\r\n");

            string[] dataChunks =
            {
                "test",
                "This is a longer text string to test how the code copes with multi digit hex length. This string is longer than 16.",
                "The end is near",
                "The end is here... \r\n almost on this second line",
                "Stop"
            };

            var body = ComposeChunkedBody(dataChunks);

            return ComposeDatagramWithBody(stringBuilder, body, isChunked:true);
        }

        private static string ComposeChunkedBody(IEnumerable<string> bodyParts)
        {
            var chunks = new StringBuilder(); //new List<byte[]>();

            foreach (var part in bodyParts)
            {
                if (part.Length > 0)
                {
                    //var stringBuilder = new StringBuilder();
                    chunks.Append(part.Length.ToString("X")); // Convert length to hex string
                    chunks.Append("\r\n");
                    chunks.Append(part);
                    chunks.Append("\r\n"); //Trailing crlf - not counted towards the length.
                    //chunks.Append(stringBuilder); //.Add(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
                }
            }

            chunks.Append($"{0:X}\r\n\r\n"); // End chunking

            return chunks.ToString();
        }

        private static byte[] ComposeDatagramWithBody(StringBuilder stringBuilder, string body, bool isChunked = false)
        {
            byte[] datagram = null;

            if (!isChunked) // Add Content-Length with message is not chunked.
            {
                if (body.Length > 0)
                {
                    stringBuilder.Append($"Content-Length: {body.Length}");
                }
                stringBuilder.Append($"\r\n\r\n");

                datagram = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            }

            if (body?.Length > 0)
            {
                datagram = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                datagram = datagram?.Concat(Encoding.UTF8.GetBytes(body)).ToArray();
            }

            return datagram;
        }

        private static string TestRequest()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("NOTIFY * HTTP/1.1\r\n");
            stringBuilder.Append("HOST: 239.255.255.250:1900\r\n");
            stringBuilder.Append("CACHE-CONTROL: max-age = 10\r\n");
            stringBuilder.Append("LOCATION: http://www.bing.com\r\n");

            stringBuilder.Append("NT: \"upnp:rootdevice\"\r\n");
            stringBuilder.Append("NTS: ssdp:alive\r\n");
            stringBuilder.Append("EXT:\r\n");
            stringBuilder.Append("SERVER: Synology/DSM/192.168.0.33 UPnP/2.0 Test/1.0\r\n");
            stringBuilder.Append("BOOTID.UPNP.ORG: 1\r\n");
            stringBuilder.Append("CONFIGID.UPNP.ORG: 121212\r\n");
            stringBuilder.Append("Content-Length: 6\r\n");
            stringBuilder.Append("\r\n");
            stringBuilder.Append("Data\r\n");
            stringBuilder.Append("\r\n");
            return stringBuilder.ToString();
        }


    }
}
