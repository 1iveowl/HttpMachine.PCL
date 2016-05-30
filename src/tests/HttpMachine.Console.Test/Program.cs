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
            var bArray = Encoding.UTF8.GetBytes(TestReponse());
            var responseHandler = new ParserHandler();

            var parserHander = new HttpParser(responseHandler);

            if (parserHander.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) == bArray.Length)
            {
                var t = "ok";
            }
            else
            {
                var s = "fail";
            }

            // Simulate error
            //var requestHandler = new ParserHandler();

            //var parserHander2 = new HttpParser(requestHandler);

            //if (parserHander2.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) == bArray.Length)
            //{
            //    var t = "ok";
            //}
            //else
            //{
            //    var s = "fail";
            //}
        }



        private static string TestReponse()
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
            stringBuilder.Append("\r\n");
            return stringBuilder.ToString();
        }
    }
}
