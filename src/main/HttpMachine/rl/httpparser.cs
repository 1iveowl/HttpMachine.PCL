
#line 1 "httpparser2-chunked.cs.rl"
﻿using System;
using System.Text;
using System.Diagnostics;

namespace HttpMachine
{
    public class HttpCombinedParser
    {
        public int MajorVersion {get; private set;}
        public int MinorVersion {get; private set;}

        public bool ShouldKeepAlive => (MajorVersion > 0 && MinorVersion > 0) ? !gotConnectionClose : gotConnectionClose;
        
        private readonly IHttpParserCombinedDelegate parserDelegate;

		private readonly StringBuilder _stringBuilder;
		private StringBuilder _stringBuilder2;
		private StringBuilder _chunkedBufferBuilder;
		private StringBuilder _chunkedHexBufferBuilder;
		
        private int _contentLength;
        private int _chunkLength;
		private int _chunkPos;

		// TODO make flags or something, dang
		private bool inContentLengthHeader;
		private bool inConnectionHeader;
		private bool inTransferEncodingHeader;
		private bool inUpgradeHeader;
		private bool gotConnectionClose;
		private bool gotConnectionKeepAlive;
		private bool gotTransferEncodingChunked;
		private bool gotUpgradeValue;

        private int cs;
        // int mark;
        private int statusCode;
        private string statusReason;

        
#line 402 "httpparser2-chunked.cs.rl"

        
        
#line 48 "httpparser.cs"
static readonly sbyte[] _http_parser_actions =  new sbyte [] {
	0, 1, 0, 1, 4, 1, 6, 1, 
	12, 1, 14, 1, 15, 1, 17, 1, 
	20, 1, 22, 1, 23, 1, 25, 1, 
	26, 1, 34, 1, 35, 1, 37, 1, 
	38, 1, 39, 1, 40, 1, 41, 2, 
	1, 0, 2, 1, 35, 2, 2, 0, 
	2, 7, 6, 2, 8, 15, 2, 16, 
	12, 2, 17, 22, 2, 18, 0, 2, 
	18, 17, 2, 19, 0, 2, 19, 17, 
	2, 24, 17, 2, 27, 34, 2, 28, 
	34, 2, 29, 35, 2, 30, 35, 2, 
	31, 34, 2, 32, 35, 2, 33, 34, 
	3, 3, 2, 0, 3, 3, 19, 0, 
	3, 3, 19, 17, 3, 3, 24, 17, 
	3, 8, 1, 0, 3, 13, 1, 0, 
	3, 18, 17, 22, 3, 19, 17, 22, 
	3, 20, 1, 0, 3, 21, 1, 0, 
	3, 23, 1, 0, 3, 24, 17, 22, 
	3, 36, 5, 4, 3, 36, 5, 39, 
	4, 3, 19, 17, 22, 4, 3, 24, 
	17, 22, 4, 13, 1, 11, 0, 4, 
	13, 1, 11, 17, 5, 13, 1, 9, 
	11, 0, 5, 13, 1, 11, 17, 22, 
	6, 13, 1, 10, 3, 2, 0
};

static readonly short[] _http_parser_key_offsets =  new short [] {
	0, 0, 10, 11, 20, 29, 44, 45, 
	67, 68, 84, 92, 93, 98, 100, 102, 
	104, 106, 108, 109, 111, 113, 115, 116, 
	119, 121, 123, 124, 126, 128, 130, 132, 
	134, 135, 153, 171, 191, 209, 227, 245, 
	263, 281, 299, 315, 333, 351, 369, 385, 
	403, 421, 439, 457, 475, 493, 509, 527, 
	545, 563, 581, 599, 617, 635, 651, 669, 
	687, 705, 723, 741, 759, 777, 795, 811, 
	829, 847, 865, 883, 901, 919, 935, 936, 
	937, 938, 939, 940, 942, 943, 945, 946, 
	961, 967, 973, 988, 1001, 1014, 1020, 1026, 
	1032, 1038, 1052, 1066, 1072, 1078, 1099, 1120, 
	1133, 1139, 1145, 1154, 1163, 1172, 1181, 1190, 
	1199, 1208, 1217, 1226, 1235, 1244, 1253, 1262, 
	1271, 1280, 1289, 1298, 1307, 1316, 1325, 1334, 
	1343, 1344, 1354, 1364, 1374, 1384, 1386, 1387, 
	1389, 1390, 1392, 1396, 1397, 1419, 1425, 1430, 
	1430, 1436, 1443, 1444, 1444, 1444, 1444, 1444, 
	1444, 1444
};

static readonly char[] _http_parser_trans_keys =  new char [] {
	'\u000d', '\u0024', '\u0048', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u000a', '\u0024', '\u0048', '\u005f', '\u002d', '\u002e', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', 
	'\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u000d', '\u0020', '\u0021', 
	'\u0025', '\u002f', '\u003d', '\u0040', '\u005f', '\u007e', '\u0024', '\u003b', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u000a', '\u000d', '\u0021', '\u0043', 
	'\u0054', '\u0055', '\u0063', '\u0074', '\u0075', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u000a', '\u0021', '\u003a', '\u007c', '\u007e', 
	'\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', 
	'\u0041', '\u005a', '\u005e', '\u007a', '\u0009', '\u000a', '\u000d', '\u0020', 
	'\u0043', '\u004b', '\u0063', '\u006b', '\u000d', '\u000d', '\u0048', '\u004c', 
	'\u0068', '\u006c', '\u0055', '\u0075', '\u004e', '\u006e', '\u004b', '\u006b', 
	'\u0045', '\u0065', '\u0044', '\u0064', '\u000d', '\u004f', '\u006f', '\u0053', 
	'\u0073', '\u0045', '\u0065', '\u000d', '\u000d', '\u0045', '\u0065', '\u0045', 
	'\u0065', '\u0050', '\u0070', '\u002d', '\u0041', '\u0061', '\u004c', '\u006c', 
	'\u0049', '\u0069', '\u0056', '\u0076', '\u0045', '\u0065', '\u000d', '\u0021', 
	'\u003a', '\u004f', '\u006f', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u004e', '\u006e', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', '\u0054', '\u006e', 
	'\u0074', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0045', '\u0065', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0043', '\u0063', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0054', '\u0074', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0049', 
	'\u0069', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u004f', '\u006f', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u004e', '\u006e', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0045', '\u0065', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', 
	'\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0054', '\u0074', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u002d', '\u002e', '\u003a', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u004c', '\u006c', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0045', '\u0065', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', 
	'\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0047', '\u0067', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0054', '\u0074', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0048', '\u0068', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0052', 
	'\u0072', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0041', '\u0061', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0042', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u004e', '\u006e', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0053', '\u0073', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0046', 
	'\u0066', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0045', '\u0065', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0052', '\u0072', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u002d', '\u002e', '\u003a', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0045', '\u0065', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', 
	'\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0043', '\u0063', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u004f', '\u006f', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0044', '\u0064', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0049', 
	'\u0069', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u004e', '\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0047', '\u0067', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0050', '\u0070', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0047', 
	'\u0067', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0052', '\u0072', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0041', '\u0061', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0042', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0044', '\u0064', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0045', 
	'\u0065', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0048', 
	'\u0054', '\u0054', '\u0050', '\u002f', '\u0030', '\u0039', '\u002e', '\u0030', 
	'\u0039', '\u000d', '\u000d', '\u0020', '\u0021', '\u0025', '\u003d', '\u005f', 
	'\u007e', '\u0024', '\u002e', '\u0030', '\u003b', '\u0040', '\u005a', '\u0061', 
	'\u007a', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u0030', 
	'\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000d', '\u0020', '\u0021', 
	'\u0023', '\u0025', '\u003d', '\u003f', '\u005f', '\u007e', '\u0024', '\u003b', 
	'\u0040', '\u005a', '\u0061', '\u007a', '\u000d', '\u0020', '\u0021', '\u0025', 
	'\u003d', '\u005f', '\u007e', '\u0024', '\u003b', '\u003f', '\u005a', '\u0061', 
	'\u007a', '\u000d', '\u0020', '\u0021', '\u0025', '\u003d', '\u005f', '\u007e', 
	'\u0024', '\u003b', '\u003f', '\u005a', '\u0061', '\u007a', '\u0030', '\u0039', 
	'\u0041', '\u0046', '\u0061', '\u0066', '\u0030', '\u0039', '\u0041', '\u0046', 
	'\u0061', '\u0066', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', 
	'\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000d', '\u0020', 
	'\u0021', '\u0023', '\u0025', '\u003d', '\u005f', '\u007e', '\u0024', '\u003b', 
	'\u003f', '\u005a', '\u0061', '\u007a', '\u000d', '\u0020', '\u0021', '\u0023', 
	'\u0025', '\u003d', '\u005f', '\u007e', '\u0024', '\u003b', '\u003f', '\u005a', 
	'\u0061', '\u007a', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', 
	'\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000d', '\u0020', 
	'\u0021', '\u0025', '\u002b', '\u003d', '\u0040', '\u005f', '\u007e', '\u0024', 
	'\u002c', '\u002d', '\u002e', '\u0030', '\u0039', '\u003a', '\u003b', '\u0041', 
	'\u005a', '\u0061', '\u007a', '\u000d', '\u0020', '\u0021', '\u0025', '\u002b', 
	'\u003a', '\u003b', '\u003d', '\u0040', '\u005f', '\u007e', '\u0024', '\u002c', 
	'\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u0061', '\u007a', 
	'\u000d', '\u0020', '\u0021', '\u0025', '\u003d', '\u005f', '\u007e', '\u0024', 
	'\u003b', '\u003f', '\u005a', '\u0061', '\u007a', '\u0030', '\u0039', '\u0041', 
	'\u0046', '\u0061', '\u0066', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', 
	'\u0066', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', '\u0041', 
	'\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', 
	'\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', 
	'\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', 
	'\u005f', '\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', 
	'\u0024', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', 
	'\u0020', '\u0024', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', '\u0061', 
	'\u007a', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', '\u0041', 
	'\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', 
	'\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', 
	'\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', 
	'\u005f', '\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', 
	'\u0024', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', 
	'\u0020', '\u0024', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', '\u0061', 
	'\u007a', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', '\u0041', 
	'\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', 
	'\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', 
	'\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', 
	'\u005f', '\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', 
	'\u0020', '\u0024', '\u0054', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u0020', '\u0024', '\u0054', '\u005f', '\u002d', '\u002e', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u0050', '\u005f', 
	'\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', 
	'\u002f', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', 
	'\u0030', '\u0039', '\u002e', '\u0030', '\u0039', '\u0020', '\u0030', '\u0039', 
	'\u000d', '\u0020', '\u0030', '\u0039', '\u000a', '\u000d', '\u0021', '\u0043', 
	'\u0054', '\u0055', '\u0063', '\u0074', '\u0075', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0000', '\u0009', '\u000b', '\u000c', '\u000e', 
	'\u007f', '\u000d', '\u0000', '\u0009', '\u000b', '\u007f', '\u0030', '\u0039', 
	'\u0041', '\u0046', '\u0061', '\u0066', '\u000d', '\u0030', '\u0039', '\u0041', 
	'\u0046', '\u0061', '\u0066', '\u000a', (char) 0
};

static readonly sbyte[] _http_parser_single_lengths =  new sbyte [] {
	0, 4, 1, 3, 3, 9, 1, 10, 
	1, 4, 8, 1, 5, 2, 2, 2, 
	2, 2, 1, 2, 2, 2, 1, 3, 
	2, 2, 1, 2, 2, 2, 2, 2, 
	1, 6, 6, 8, 6, 6, 6, 6, 
	6, 6, 4, 6, 6, 6, 6, 6, 
	6, 6, 6, 6, 6, 4, 6, 6, 
	6, 6, 6, 6, 6, 6, 6, 6, 
	6, 6, 6, 6, 6, 6, 4, 6, 
	6, 6, 6, 6, 6, 4, 1, 1, 
	1, 1, 1, 0, 1, 0, 1, 7, 
	0, 0, 9, 7, 7, 0, 0, 0, 
	0, 8, 8, 0, 0, 9, 11, 7, 
	0, 0, 3, 3, 3, 3, 3, 3, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	1, 4, 4, 4, 4, 0, 1, 0, 
	1, 0, 2, 1, 10, 0, 1, 0, 
	0, 1, 1, 0, 0, 0, 0, 0, 
	0, 0
};

static readonly sbyte[] _http_parser_range_lengths =  new sbyte [] {
	0, 3, 0, 3, 3, 3, 0, 6, 
	0, 6, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 6, 6, 6, 6, 6, 6, 6, 
	6, 6, 6, 6, 6, 6, 5, 6, 
	6, 6, 6, 6, 6, 6, 6, 6, 
	6, 6, 6, 6, 6, 5, 6, 6, 
	6, 6, 6, 6, 6, 6, 6, 6, 
	6, 6, 6, 6, 6, 6, 0, 0, 
	0, 0, 0, 1, 0, 1, 0, 4, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	3, 3, 3, 3, 3, 6, 5, 3, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	0, 3, 3, 3, 3, 1, 0, 1, 
	0, 1, 1, 0, 6, 3, 2, 0, 
	3, 3, 0, 0, 0, 0, 0, 0, 
	0, 0
};

static readonly short[] _http_parser_index_offsets =  new short [] {
	0, 0, 8, 10, 17, 24, 37, 39, 
	56, 58, 69, 78, 80, 86, 89, 92, 
	95, 98, 101, 103, 106, 109, 112, 114, 
	118, 121, 124, 126, 129, 132, 135, 138, 
	141, 143, 156, 169, 184, 197, 210, 223, 
	236, 249, 262, 273, 286, 299, 312, 324, 
	337, 350, 363, 376, 389, 402, 413, 426, 
	439, 452, 465, 478, 491, 504, 516, 529, 
	542, 555, 568, 581, 594, 607, 620, 631, 
	644, 657, 670, 683, 696, 709, 720, 722, 
	724, 726, 728, 730, 732, 734, 736, 738, 
	750, 754, 758, 771, 782, 793, 797, 801, 
	805, 809, 821, 833, 837, 841, 857, 874, 
	885, 889, 893, 900, 907, 914, 921, 928, 
	935, 942, 949, 956, 963, 970, 977, 984, 
	991, 998, 1005, 1012, 1019, 1026, 1033, 1040, 
	1047, 1049, 1057, 1065, 1073, 1081, 1083, 1085, 
	1087, 1089, 1091, 1095, 1097, 1114, 1118, 1122, 
	1123, 1127, 1132, 1134, 1135, 1136, 1137, 1138, 
	1139, 1140
};

static readonly byte[] _http_parser_indicies =  new byte [] {
	0, 2, 3, 2, 2, 2, 2, 1, 
	4, 1, 5, 6, 5, 5, 5, 5, 
	1, 7, 8, 8, 8, 8, 8, 1, 
	9, 10, 11, 12, 13, 11, 11, 11, 
	11, 11, 14, 14, 1, 15, 1, 16, 
	17, 18, 19, 20, 18, 19, 20, 17, 
	17, 17, 17, 17, 17, 17, 17, 1, 
	21, 1, 22, 23, 22, 22, 22, 22, 
	22, 22, 22, 22, 1, 25, 1, 26, 
	25, 27, 28, 27, 28, 24, 29, 1, 
	29, 30, 31, 30, 31, 1, 32, 32, 
	1, 33, 33, 1, 34, 34, 1, 35, 
	35, 1, 36, 36, 1, 37, 1, 38, 
	38, 1, 39, 39, 1, 40, 40, 1, 
	41, 1, 29, 42, 42, 1, 43, 43, 
	1, 44, 44, 1, 45, 1, 46, 46, 
	1, 47, 47, 1, 48, 48, 1, 49, 
	49, 1, 50, 50, 1, 51, 1, 22, 
	23, 52, 52, 22, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 23, 53, 53, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 54, 55, 54, 55, 22, 
	22, 22, 22, 22, 22, 22, 22, 1, 
	22, 23, 56, 56, 22, 22, 22, 22, 
	22, 22, 22, 22, 1, 22, 23, 57, 
	57, 22, 22, 22, 22, 22, 22, 22, 
	22, 1, 22, 23, 58, 58, 22, 22, 
	22, 22, 22, 22, 22, 22, 1, 22, 
	23, 59, 59, 22, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 23, 60, 60, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 61, 61, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 62, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 63, 63, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 23, 
	64, 64, 22, 22, 22, 22, 22, 22, 
	22, 22, 1, 22, 23, 65, 65, 22, 
	22, 22, 22, 22, 22, 22, 22, 1, 
	22, 66, 22, 23, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 23, 67, 67, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 68, 68, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 23, 
	69, 69, 22, 22, 22, 22, 22, 22, 
	22, 22, 1, 22, 23, 70, 70, 22, 
	22, 22, 22, 22, 22, 22, 22, 1, 
	22, 23, 71, 71, 22, 22, 22, 22, 
	22, 22, 22, 22, 1, 22, 23, 72, 
	72, 22, 22, 22, 22, 22, 22, 22, 
	22, 1, 22, 73, 22, 22, 22, 22, 
	22, 22, 22, 22, 1, 22, 23, 74, 
	74, 22, 22, 22, 22, 22, 22, 22, 
	22, 1, 22, 23, 75, 75, 22, 22, 
	22, 22, 22, 22, 22, 22, 1, 22, 
	23, 76, 76, 22, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 23, 77, 77, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 78, 78, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 23, 
	79, 79, 22, 22, 22, 22, 22, 22, 
	22, 22, 1, 22, 23, 80, 80, 22, 
	22, 22, 22, 22, 22, 22, 22, 1, 
	22, 81, 22, 23, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 23, 82, 82, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 83, 83, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 23, 
	84, 84, 22, 22, 22, 22, 22, 22, 
	22, 22, 1, 22, 23, 85, 85, 22, 
	22, 22, 22, 22, 22, 22, 22, 1, 
	22, 23, 86, 86, 22, 22, 22, 22, 
	22, 22, 22, 22, 1, 22, 23, 87, 
	87, 22, 22, 22, 22, 22, 22, 22, 
	22, 1, 22, 23, 88, 88, 22, 22, 
	22, 22, 22, 22, 22, 22, 1, 22, 
	23, 89, 89, 22, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 90, 22, 22, 
	22, 22, 22, 22, 22, 22, 1, 22, 
	23, 91, 91, 22, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 23, 92, 92, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 93, 93, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 23, 
	94, 94, 22, 22, 22, 22, 22, 22, 
	22, 22, 1, 22, 23, 95, 95, 22, 
	22, 22, 22, 22, 22, 22, 22, 1, 
	22, 23, 96, 96, 22, 22, 22, 22, 
	22, 22, 22, 22, 1, 22, 97, 22, 
	22, 22, 22, 22, 22, 22, 22, 1, 
	98, 1, 99, 1, 100, 1, 101, 1, 
	102, 1, 103, 1, 104, 1, 105, 1, 
	106, 1, 107, 108, 109, 110, 109, 109, 
	109, 109, 109, 109, 109, 1, 111, 111, 
	111, 1, 109, 109, 109, 1, 112, 113, 
	114, 115, 116, 114, 117, 114, 114, 114, 
	114, 114, 1, 118, 119, 120, 121, 120, 
	120, 120, 120, 120, 120, 1, 122, 123, 
	124, 125, 124, 124, 124, 124, 124, 124, 
	1, 126, 126, 126, 1, 124, 124, 124, 
	1, 127, 127, 127, 1, 114, 114, 114, 
	1, 128, 129, 130, 131, 132, 130, 130, 
	130, 130, 130, 130, 1, 133, 134, 135, 
	136, 137, 135, 135, 135, 135, 135, 135, 
	1, 138, 138, 138, 1, 135, 135, 135, 
	1, 107, 108, 109, 110, 139, 109, 109, 
	109, 109, 109, 139, 139, 109, 139, 139, 
	1, 107, 108, 109, 110, 139, 140, 109, 
	109, 109, 109, 109, 109, 139, 139, 139, 
	139, 1, 107, 108, 140, 141, 140, 140, 
	140, 140, 140, 140, 1, 142, 142, 142, 
	1, 140, 140, 140, 1, 7, 143, 143, 
	143, 143, 143, 1, 7, 144, 144, 144, 
	144, 144, 1, 7, 145, 145, 145, 145, 
	145, 1, 7, 146, 146, 146, 146, 146, 
	1, 7, 147, 147, 147, 147, 147, 1, 
	7, 148, 148, 148, 148, 148, 1, 7, 
	149, 149, 149, 149, 149, 1, 7, 150, 
	150, 150, 150, 150, 1, 7, 151, 151, 
	151, 151, 151, 1, 7, 152, 152, 152, 
	152, 152, 1, 7, 153, 153, 153, 153, 
	153, 1, 7, 154, 154, 154, 154, 154, 
	1, 7, 155, 155, 155, 155, 155, 1, 
	7, 156, 156, 156, 156, 156, 1, 7, 
	157, 157, 157, 157, 157, 1, 7, 158, 
	158, 158, 158, 158, 1, 7, 159, 159, 
	159, 159, 159, 1, 7, 160, 160, 160, 
	160, 160, 1, 7, 161, 161, 161, 161, 
	161, 1, 7, 162, 162, 162, 162, 162, 
	1, 7, 163, 163, 163, 163, 163, 1, 
	7, 164, 164, 164, 164, 164, 1, 7, 
	1, 7, 8, 165, 8, 8, 8, 8, 
	1, 7, 143, 166, 143, 143, 143, 143, 
	1, 7, 144, 167, 144, 144, 144, 144, 
	1, 7, 145, 168, 145, 145, 145, 145, 
	1, 169, 1, 170, 1, 171, 1, 172, 
	1, 173, 1, 174, 175, 176, 1, 177, 
	1, 178, 179, 180, 181, 182, 180, 181, 
	182, 179, 179, 179, 179, 179, 179, 179, 
	179, 1, 183, 183, 183, 1, 185, 184, 
	184, 1, 186, 187, 187, 187, 1, 188, 
	189, 189, 189, 1, 190, 1, 191, 1, 
	186, 192, 193, 194, 1, 0
};

static readonly byte[] _http_parser_trans_targs =  new byte [] {
	2, 0, 4, 129, 3, 4, 129, 5, 
	106, 6, 78, 87, 88, 90, 101, 7, 
	8, 9, 33, 54, 71, 148, 9, 10, 
	11, 10, 6, 12, 23, 6, 13, 19, 
	14, 15, 16, 17, 18, 6, 20, 21, 
	22, 6, 24, 25, 26, 27, 28, 29, 
	30, 31, 32, 6, 34, 35, 36, 43, 
	37, 38, 39, 40, 41, 42, 10, 44, 
	45, 46, 47, 48, 49, 50, 51, 52, 
	53, 10, 55, 56, 57, 58, 59, 60, 
	61, 62, 63, 64, 65, 66, 67, 68, 
	69, 70, 10, 72, 73, 74, 75, 76, 
	77, 10, 79, 80, 81, 82, 83, 84, 
	85, 86, 6, 6, 78, 87, 88, 89, 
	6, 78, 90, 91, 95, 97, 6, 78, 
	92, 93, 6, 78, 92, 93, 94, 96, 
	6, 78, 98, 91, 99, 6, 78, 98, 
	91, 99, 100, 102, 103, 104, 105, 107, 
	108, 109, 110, 111, 112, 113, 114, 115, 
	116, 117, 118, 119, 120, 121, 122, 123, 
	124, 125, 126, 127, 128, 130, 131, 132, 
	133, 134, 135, 136, 137, 138, 139, 141, 
	138, 140, 8, 9, 33, 54, 71, 142, 
	142, 139, 149, 145, 146, 145, 151, 153, 
	150, 152, 152
};

static readonly byte[] _http_parser_trans_actions =  new byte [] {
	51, 0, 112, 112, 11, 39, 39, 54, 
	1, 178, 167, 162, 162, 184, 172, 0, 
	0, 39, 39, 39, 39, 29, 1, 25, 
	39, 0, 42, 39, 39, 27, 1, 1, 
	1, 1, 1, 1, 1, 90, 1, 1, 
	1, 81, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 84, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 78, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 75, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 87, 1, 1, 1, 1, 1, 
	1, 93, 0, 0, 0, 0, 0, 21, 
	0, 23, 17, 57, 13, 1, 1, 1, 
	120, 63, 45, 60, 45, 60, 157, 108, 
	96, 96, 140, 72, 45, 45, 45, 45, 
	152, 104, 96, 100, 96, 124, 69, 45, 
	66, 45, 45, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	0, 21, 0, 23, 7, 116, 128, 15, 
	1, 1, 19, 136, 136, 136, 136, 39, 
	1, 132, 31, 48, 5, 5, 5, 0, 
	35, 144, 3
};

static readonly byte[] _http_parser_from_state_actions =  new byte [] {
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 37
};

static readonly byte[] _http_parser_eof_actions =  new byte [] {
	0, 0, 0, 0, 0, 9, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 9, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 148, 
	33, 0
};

const int http_parser_start = 1;
const int http_parser_first_final = 148;
const int http_parser_error = 0;

const int http_parser_en_main = 1;
const int http_parser_en_body_identity = 143;
const int http_parser_en_body_identity_eof = 150;
const int http_parser_en_body_chunked_identity = 144;
const int http_parser_en_dead = 147;


#line 405 "httpparser2-chunked.cs.rl"
        
        protected HttpCombinedParser()
        {
			_stringBuilder = new StringBuilder();
			_chunkedBufferBuilder = new StringBuilder();
			_chunkedHexBufferBuilder = new StringBuilder();
            
#line 619 "httpparser.cs"
	{
	cs = http_parser_start;
	}

#line 412 "httpparser2-chunked.cs.rl"
        }

        public HttpCombinedParser(IHttpParserCombinedDelegate del) : this()
        {
            this.parserDelegate = del;
        }
	
        public int Execute(ArraySegment<byte> buf)
        {
			byte[] data = buf.Array;
			int p = buf.Offset;
			int pe = buf.Offset + buf.Count;
			int eof = buf.Count == 0 ? buf.Offset : -1;
			
			try
			{
				
#line 642 "httpparser.cs"
	{
	sbyte _klen;
	short _trans;
	int _acts;
	int _nacts;
	short _keys;

	if ( p == pe )
		goto _test_eof;
	if ( cs == 0 )
		goto _out;
_resume:
	_acts = _http_parser_from_state_actions[cs];
	_nacts = _http_parser_actions[_acts++];
	while ( _nacts-- > 0 ) {
		switch ( _http_parser_actions[_acts++] ) {
	case 41:
#line 396 "httpparser2-chunked.cs.rl"
	{
			throw new Exception("Parser is dead; there shouldn't be more data. Client is bogus? fpc =" + p);
		}
	break;
#line 665 "httpparser.cs"
		default: break;
		}
	}

	_keys = _http_parser_key_offsets[cs];
	_trans = (short)_http_parser_index_offsets[cs];

	_klen = _http_parser_single_lengths[cs];
	if ( _klen > 0 ) {
		short _lower = _keys;
		short _mid;
		short _upper = (short) (_keys + _klen - 1);
		while (true) {
			if ( _upper < _lower )
				break;

			_mid = (short) (_lower + ((_upper-_lower) >> 1));
			if ( data[p] < _http_parser_trans_keys[_mid] )
				_upper = (short) (_mid - 1);
			else if ( data[p] > _http_parser_trans_keys[_mid] )
				_lower = (short) (_mid + 1);
			else {
				_trans += (short) (_mid - _keys);
				goto _match;
			}
		}
		_keys += (short) _klen;
		_trans += (short) _klen;
	}

	_klen = _http_parser_range_lengths[cs];
	if ( _klen > 0 ) {
		short _lower = _keys;
		short _mid;
		short _upper = (short) (_keys + (_klen<<1) - 2);
		while (true) {
			if ( _upper < _lower )
				break;

			_mid = (short) (_lower + (((_upper-_lower) >> 1) & ~1));
			if ( data[p] < _http_parser_trans_keys[_mid] )
				_upper = (short) (_mid - 2);
			else if ( data[p] > _http_parser_trans_keys[_mid+1] )
				_lower = (short) (_mid + 2);
			else {
				_trans += (short)((_mid - _keys)>>1);
				goto _match;
			}
		}
		_trans += (short) _klen;
	}

_match:
	_trans = (short)_http_parser_indicies[_trans];
	cs = _http_parser_trans_targs[_trans];

	if ( _http_parser_trans_actions[_trans] == 0 )
		goto _again;

	_acts = _http_parser_trans_actions[_trans];
	_nacts = _http_parser_actions[_acts++];
	while ( _nacts-- > 0 )
	{
		switch ( _http_parser_actions[_acts++] )
		{
	case 0:
#line 45 "httpparser2-chunked.cs.rl"
	{
			_stringBuilder.Append((char)data[p]);
		}
	break;
	case 1:
#line 49 "httpparser2-chunked.cs.rl"
	{
			_stringBuilder.Length = 0;
		}
	break;
	case 2:
#line 53 "httpparser2-chunked.cs.rl"
	{
			_stringBuilder2.Append((char)data[p]);
		}
	break;
	case 3:
#line 57 "httpparser2-chunked.cs.rl"
	{
			if (_stringBuilder2 == null)
				_stringBuilder2 = new StringBuilder();
		 	_stringBuilder2.Length = 0;
		}
	break;
	case 4:
#line 63 "httpparser2-chunked.cs.rl"
	{
			_chunkedBufferBuilder.Append((char)data[p]);
		}
	break;
	case 5:
#line 67 "httpparser2-chunked.cs.rl"
	{
			_chunkedBufferBuilder.clear();
		}
	break;
	case 6:
#line 71 "httpparser2-chunked.cs.rl"
	{
			_chunkedHexBufferBuilder.Append((char)data[p]);
		}
	break;
	case 7:
#line 75 "httpparser2-chunked.cs.rl"
	{
			_chunkedHexBufferBuilder.clear();
		}
	break;
	case 8:
#line 79 "httpparser2-chunked.cs.rl"
	{
			//Console.WriteLine("message_begin");
			MajorVersion = 0;
			MinorVersion = 9;
			_contentLength = -1;
			inContentLengthHeader = false;
			inConnectionHeader = false;
			inTransferEncodingHeader = false;
			inUpgradeHeader = false;

			gotConnectionClose = false;
			gotConnectionKeepAlive = false;
			gotTransferEncodingChunked = false;
			gotUpgradeValue = false;
			parserDelegate.OnMessageBegin(this);
		}
	break;
	case 9:
#line 96 "httpparser2-chunked.cs.rl"
	{
           //Console.WriteLine("matched absolute_uri");
        }
	break;
	case 10:
#line 99 "httpparser2-chunked.cs.rl"
	{
            //Console.WriteLine("matched abs_path");
        }
	break;
	case 11:
#line 102 "httpparser2-chunked.cs.rl"
	{
            //Console.WriteLine("matched authority");
        }
	break;
	case 12:
#line 105 "httpparser2-chunked.cs.rl"
	{
            //Console.WriteLine("matched first space");
        }
	break;
	case 13:
#line 108 "httpparser2-chunked.cs.rl"
	{
            //Console.WriteLine("leave_first_space");
        }
	break;
	case 15:
#line 117 "httpparser2-chunked.cs.rl"
	{
			//Console.WriteLine("matched_leading_crlf");
		}
	break;
	case 16:
#line 127 "httpparser2-chunked.cs.rl"
	{
			parserDelegate.OnMethod(this, _stringBuilder.ToString());
		}
	break;
	case 17:
#line 131 "httpparser2-chunked.cs.rl"
	{
			parserDelegate.OnRequestUri(this, _stringBuilder.ToString());
		}
	break;
	case 18:
#line 136 "httpparser2-chunked.cs.rl"
	{
			parserDelegate.OnPath(this, _stringBuilder2.ToString());
		}
	break;
	case 19:
#line 141 "httpparser2-chunked.cs.rl"
	{
			parserDelegate.OnQueryString(this, _stringBuilder2.ToString());
		}
	break;
	case 20:
#line 146 "httpparser2-chunked.cs.rl"
	{
			statusCode = int.Parse(_stringBuilder.ToString());
		}
	break;
	case 21:
#line 151 "httpparser2-chunked.cs.rl"
	{
			statusReason = _stringBuilder.ToString();
		}
	break;
	case 22:
#line 156 "httpparser2-chunked.cs.rl"
	{
			parserDelegate.OnRequestType(this);
		}
	break;
	case 23:
#line 161 "httpparser2-chunked.cs.rl"
	{
			parserDelegate.OnResponseType(this);
			parserDelegate.OnResponseCode(this, statusCode, statusReason);
			statusReason = null;
			statusCode = 0;
		}
	break;
	case 24:
#line 178 "httpparser2-chunked.cs.rl"
	{
			parserDelegate.OnFragment(this, _stringBuilder2.ToString());
		}
	break;
	case 25:
#line 191 "httpparser2-chunked.cs.rl"
	{
			MajorVersion = (char)data[p] - '0';
		}
	break;
	case 26:
#line 195 "httpparser2-chunked.cs.rl"
	{
			MinorVersion = (char)data[p] - '0';
		}
	break;
	case 27:
#line 199 "httpparser2-chunked.cs.rl"
	{
            if (_contentLength != -1) throw new Exception("Already got Content-Length. Possible attack?");
			//Console.WriteLine("Saw content length");
			_contentLength = 0;
			inContentLengthHeader = true;
        }
	break;
	case 28:
#line 206 "httpparser2-chunked.cs.rl"
	{
			//Console.WriteLine("header_connection");
			inConnectionHeader = true;
		}
	break;
	case 29:
#line 211 "httpparser2-chunked.cs.rl"
	{
			//Console.WriteLine("header_connection_close");
			if (inConnectionHeader)
				gotConnectionClose = true;
		}
	break;
	case 30:
#line 217 "httpparser2-chunked.cs.rl"
	{
			//Console.WriteLine("header_connection_keepalive");
			if (inConnectionHeader)
				gotConnectionKeepAlive = true;
		}
	break;
	case 31:
#line 223 "httpparser2-chunked.cs.rl"
	{
			//Console.WriteLine("Saw transfer encoding");
			inTransferEncodingHeader = true;
		}
	break;
	case 32:
#line 228 "httpparser2-chunked.cs.rl"
	{
			if (inTransferEncodingHeader)
				gotTransferEncodingChunked = true;
            parserDelegate.OnTransferEncodingChunked(this, true);
		}
	break;
	case 33:
#line 234 "httpparser2-chunked.cs.rl"
	{
			inUpgradeHeader = true;
		}
	break;
	case 34:
#line 238 "httpparser2-chunked.cs.rl"
	{
			parserDelegate.OnHeaderName(this, _stringBuilder.ToString());
		}
	break;
	case 35:
#line 242 "httpparser2-chunked.cs.rl"
	{
			var str = _stringBuilder.ToString();
			//Console.WriteLine("on_header_value '" + str + "'");
			//Console.WriteLine("inContentLengthHeader " + inContentLengthHeader);
			if (inContentLengthHeader)
				_contentLength = int.Parse(str);

			inConnectionHeader = inTransferEncodingHeader = inContentLengthHeader = false;
			
			parserDelegate.OnHeaderValue(this, str);
		}
	break;
	case 36:
#line 254 "httpparser2-chunked.cs.rl"
	{
            _chunkLength = Convert.ToInt32(_chunkedHexBufferBuilder.ToString(), 16);
			_chunkPos = _chunkLength;		
        }
	break;
	case 37:
#line 259 "httpparser2-chunked.cs.rl"
	{
			
			if (data[p] == 10)
			{
				//Console.WriteLine("leave_headers contentLength = " + contentLength);
				parserDelegate.OnHeadersEnd(this);

				// if chunked transfer, ignore content length and parse chunked (but we can't yet so bail)
				// if content length given but zero, read next request
				// if content length is given and non-zero, we should read that many bytes
				// if content length is not given
				//   if should keep alive, assume next request is coming and read it
				//   else 
				//		if chunked transfer read body until EOF
				//   	else read next request

				if (_contentLength == 0)
				{
					// No Content. Get ready for new incoming request 
					parserDelegate.OnMessageEnd(this);
					{cs = 1; if (true) goto _again;}
				}
				else if (_contentLength > 0)
				{
					// Handle Body based on Content Length
					{cs = 143; if (true) goto _again;}
				}
				else if (OnTransferEncodingChunked)
				{
					// Handle Body based on Transfer-Encoding Chunked Length
					{cs = 144; if (true) goto _again;}
				}
				else
				{
					if (ShouldKeepAlive)
					{
						parserDelegate.OnMessageEnd(this);
						{cs = 1; if (true) goto _again;}
					}
				}
			}
        }
	break;
	case 38:
#line 302 "httpparser2-chunked.cs.rl"
	{
			var toRead = Math.Min(pe - p, _contentLength);
			//Console.WriteLine("body_identity: reading " + toRead + " bytes from body.");
			if (toRead > 0)
			{
				parserDelegate.OnBody(this, new ArraySegment<byte>(data, p, toRead));
				p += toRead - 1;
				_contentLength -= toRead;
				//Console.WriteLine("content length is now " + contentLength);

				if (_contentLength == 0)
				{
					parserDelegate.OnMessageEnd(this);

					if (ShouldKeepAlive)
					{
						//Console.WriteLine("Transitioning from identity body to next message.");
						//fhold;
						{cs = 1; if (true) goto _again;}
					}
					else
					{
						//fhold;
						{cs = 147; if (true) goto _again;}
					}
				}
				else
				{
					{p++; if (true) goto _out; }
				}
			}
		}
	break;
	case 40:
#line 370 "httpparser2-chunked.cs.rl"
	{
			var toRead = pe - p;
			//Console.WriteLine("body_identity_eof: reading " + toRead + " bytes from body.");
			if (toRead > 0)
			{
				parserDelegate.OnBody(this, new ArraySegment<byte>(data, p, toRead));
				p += toRead - 1;
				{p++; if (true) goto _out; }
			}
			else
			{
				parserDelegate.OnMessageEnd(this);
				
				if (ShouldKeepAlive)
					{cs = 1; if (true) goto _again;}
				else
				{
					//Console.WriteLine("body_identity_eof: going to dead");
					p--;
					{cs = 147; if (true) goto _again;}
				}
			}
		}
	break;
#line 1091 "httpparser.cs"
		default: break;
		}
	}

_again:
	if ( cs == 0 )
		goto _out;
	if ( ++p != pe )
		goto _resume;
	_test_eof: {}
	if ( p == eof )
	{
	int __acts = _http_parser_eof_actions[cs];
	int __nacts = _http_parser_actions[__acts++];
	while ( __nacts-- > 0 ) {
		switch ( _http_parser_actions[__acts++] ) {
	case 5:
#line 67 "httpparser2-chunked.cs.rl"
	{
			_chunkedBufferBuilder.clear();
		}
	break;
	case 14:
#line 111 "httpparser2-chunked.cs.rl"
	{
            //Console.WriteLine("eof_leave_first_space");
        }
	break;
	case 36:
#line 254 "httpparser2-chunked.cs.rl"
	{
            _chunkLength = Convert.ToInt32(_chunkedHexBufferBuilder.ToString(), 16);
			_chunkPos = _chunkLength;		
        }
	break;
	case 39:
#line 337 "httpparser2-chunked.cs.rl"
	{
			
			if (_chunkLength == 0) //If zero chunk length tThen this is the last chunk
			{
				{cs = 150; if (true) goto _again;}
				// parserDelegate.OnMessageEnd(this);

				// if (ShouldKeepAlive)
				// {
				// 	fgoto main;
				// }
				// else
				// {
				// 	fgoto dead;
				// }
			}
			else
			{
				var toRead = Math.Min(pe - p, _chunkPos);
				//Console.WriteLine("body_identity: reading " + toRead + " bytes from body.");
				if (toRead > 0)
				{
					parserDelegate.OnBody(this, new ArraySegment<byte>(data, p, toRead));
					p += toRead - 1;
					_chunkPos -= toRead;
				}
				if (_chunkPos == 0) // When zero we are done reading this chuck
				{
					{cs = 144; if (true) goto _again;} //Get ready to read next chunk.
				}
			}
		}
	break;
#line 1162 "httpparser.cs"
		default: break;
		}
	}
	}

	_out: {}
	}

#line 429 "httpparser2-chunked.cs.rl"
			}
			catch (Exception)
			{
                parserDelegate.OnParserError();
			}			
							
			var result = p - buf.Offset;

			if (result != buf.Count)
			{
				Debug.WriteLine("error on character " + p);
				Debug.WriteLine("('" + buf.Array[p] + "')");
				Debug.WriteLine("('" + (char)buf.Array[p] + "')");
			}
			
			return p - buf.Offset;            
        }
    }
}