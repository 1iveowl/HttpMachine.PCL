
#line 1 "HttpParser2-chunked.cs.rl"
﻿using System;
using System.Buffers;
using System.Text;
using System.Diagnostics;
using IHttpMachine;
using System.IO;

namespace HttpMachine;

/// <summary>
/// Combined HTTP request/response parser built on a Ragel-generated state machine.
/// Feed it data with the <c>Execute</c> overloads; results arrive through the callbacks on
/// the <see cref="IHttpParserCombinedDelegate"/> supplied to the constructor.
/// This class is generated from <c>rl/HttpParser2-chunked.cs.rl</c> — edit that file, not the .cs.
/// </summary>
public class HttpCombinedParser : IHttpCombinedParser, IDisposable
{
    /// <inheritdoc/>
    public int MajorVersion {get; private set;}

    /// <inheritdoc/>
    public int MinorVersion {get; private set;}

    /// <inheritdoc/>
    public bool ShouldKeepAlive => (MajorVersion > 0 && MinorVersion > 0)
			? !gotConnectionClose
			: gotConnectionKeepAlive;

    private readonly IHttpParserCombinedDelegate parserDelegate;
    private readonly IHttpParserSpanDelegate spanDelegate;

		private readonly StringBuilder _stringBuilder;
		private StringBuilder _stringBuilder2;

    private int _contentLength;
    private int _chunkLength;

		// TODO make flags or something, dang
		private bool inContentLengthHeader;
		private bool inConnectionHeader;
		private bool inTransferEncodingHeader;
		private bool inUpgradeHeader;
		private bool gotConnectionClose;
		private bool gotConnectionKeepAlive;
		private bool gotTransferEncodingChunked;
		private bool _isRequest;

    private int cs;
    // int mark;
    private int statusCode;
    private string statusReason;

		/// <summary>Releases resources. The parser holds none.</summary>
		public void Dispose()
		{

		}

    
#line 451 "HttpParser2-chunked.cs.rl"


    
#line 62 "../HttpCombinedParser.cs"
static readonly sbyte[] _http_parser_actions =  new sbyte [] {
	0, 1, 0, 1, 4, 1, 10, 1, 
	12, 1, 13, 1, 15, 1, 19, 1, 
	21, 1, 22, 1, 24, 1, 25, 1, 
	33, 1, 34, 1, 35, 1, 36, 1, 
	37, 1, 38, 1, 39, 1, 40, 1, 
	41, 2, 1, 0, 2, 1, 34, 2, 
	2, 0, 2, 5, 4, 2, 6, 13, 
	2, 14, 10, 2, 15, 21, 2, 16, 
	0, 2, 16, 15, 2, 17, 0, 2, 
	17, 15, 2, 23, 15, 2, 26, 33, 
	2, 27, 33, 2, 28, 34, 2, 29, 
	34, 2, 30, 33, 2, 31, 34, 2, 
	32, 33, 2, 38, 39, 3, 3, 2, 
	0, 3, 3, 17, 0, 3, 3, 17, 
	15, 3, 3, 23, 15, 3, 6, 1, 
	0, 3, 11, 18, 19, 3, 16, 15, 
	21, 3, 17, 15, 21, 3, 20, 1, 
	0, 3, 22, 1, 0, 3, 23, 15, 
	21, 4, 3, 17, 15, 21, 4, 3, 
	23, 15, 21, 4, 11, 1, 9, 0, 
	4, 11, 1, 9, 15, 5, 11, 1, 
	7, 9, 0, 5, 11, 1, 9, 15, 
	21, 6, 11, 1, 8, 3, 2, 0
	
};

static readonly short[] _http_parser_key_offsets =  new short [] {
	0, 0, 10, 11, 20, 29, 44, 45, 
	67, 68, 84, 92, 94, 100, 104, 108, 
	112, 116, 120, 122, 126, 130, 134, 136, 
	140, 144, 148, 151, 155, 159, 163, 167, 
	171, 173, 191, 209, 229, 247, 265, 283, 
	301, 319, 337, 353, 371, 389, 407, 423, 
	441, 459, 477, 495, 513, 531, 547, 565, 
	583, 601, 619, 637, 655, 673, 689, 707, 
	725, 743, 761, 779, 797, 815, 833, 849, 
	867, 885, 903, 921, 939, 957, 973, 974, 
	975, 976, 977, 978, 980, 981, 983, 984, 
	999, 1005, 1011, 1026, 1039, 1052, 1058, 1064, 
	1070, 1076, 1090, 1104, 1110, 1116, 1137, 1158, 
	1171, 1177, 1183, 1192, 1201, 1210, 1219, 1228, 
	1237, 1246, 1255, 1264, 1273, 1282, 1291, 1300, 
	1309, 1318, 1327, 1336, 1345, 1354, 1363, 1372, 
	1381, 1382, 1392, 1402, 1412, 1422, 1424, 1425, 
	1427, 1428, 1430, 1434, 1435, 1457, 1463, 1468, 
	1468, 1475, 1476, 1482, 1489, 1490, 1490, 1490, 
	1490, 1490, 1490, 1490, 1490
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
	'\u0043', '\u004b', '\u0063', '\u006b', '\u000a', '\u000d', '\u000a', '\u000d', 
	'\u0048', '\u004c', '\u0068', '\u006c', '\u000a', '\u000d', '\u0055', '\u0075', 
	'\u000a', '\u000d', '\u004e', '\u006e', '\u000a', '\u000d', '\u004b', '\u006b', 
	'\u000a', '\u000d', '\u0045', '\u0065', '\u000a', '\u000d', '\u0044', '\u0064', 
	'\u000a', '\u000d', '\u000a', '\u000d', '\u004f', '\u006f', '\u000a', '\u000d', 
	'\u0053', '\u0073', '\u000a', '\u000d', '\u0045', '\u0065', '\u000a', '\u000d', 
	'\u000a', '\u000d', '\u0045', '\u0065', '\u000a', '\u000d', '\u0045', '\u0065', 
	'\u000a', '\u000d', '\u0050', '\u0070', '\u000a', '\u000d', '\u002d', '\u000a', 
	'\u000d', '\u0041', '\u0061', '\u000a', '\u000d', '\u004c', '\u006c', '\u000a', 
	'\u000d', '\u0049', '\u0069', '\u000a', '\u000d', '\u0056', '\u0076', '\u000a', 
	'\u000d', '\u0045', '\u0065', '\u000a', '\u000d', '\u0021', '\u003a', '\u004f', 
	'\u006f', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u004e', '\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u004e', '\u0054', '\u006e', '\u0074', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0045', 
	'\u0065', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0043', '\u0063', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0054', '\u0074', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0049', '\u0069', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004f', 
	'\u006f', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u004e', '\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0045', '\u0065', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', '\u006e', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0054', 
	'\u0074', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u002d', '\u002e', '\u003a', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u004c', '\u006c', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0045', '\u0065', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', '\u006e', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0047', 
	'\u0067', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0054', '\u0074', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0048', '\u0068', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0052', '\u0072', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0041', 
	'\u0061', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0042', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u004e', '\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0053', '\u0073', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0046', '\u0066', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0045', 
	'\u0065', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0052', '\u0072', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u002d', '\u002e', '\u003a', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0045', '\u0065', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', '\u006e', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0043', 
	'\u0063', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u004f', '\u006f', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0044', '\u0064', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0049', '\u0069', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u004e', 
	'\u006e', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0047', '\u0067', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0050', '\u0070', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0047', '\u0067', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0052', 
	'\u0072', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', 
	'\u003a', '\u0041', '\u0061', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0042', '\u005a', '\u005e', 
	'\u007a', '\u0021', '\u003a', '\u0044', '\u0064', '\u007c', '\u007e', '\u0023', 
	'\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', 
	'\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u0045', '\u0065', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0021', '\u003a', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0048', '\u0054', '\u0054', 
	'\u0050', '\u002f', '\u0030', '\u0039', '\u002e', '\u0030', '\u0039', '\u000d', 
	'\u000d', '\u0020', '\u0021', '\u0025', '\u003d', '\u005f', '\u007e', '\u0024', 
	'\u002e', '\u0030', '\u003b', '\u0040', '\u005a', '\u0061', '\u007a', '\u0030', 
	'\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u0030', '\u0039', '\u0041', 
	'\u0046', '\u0061', '\u0066', '\u000d', '\u0020', '\u0021', '\u0023', '\u0025', 
	'\u003d', '\u003f', '\u005f', '\u007e', '\u0024', '\u003b', '\u0040', '\u005a', 
	'\u0061', '\u007a', '\u000d', '\u0020', '\u0021', '\u0025', '\u003d', '\u005f', 
	'\u007e', '\u0024', '\u003b', '\u003f', '\u005a', '\u0061', '\u007a', '\u000d', 
	'\u0020', '\u0021', '\u0025', '\u003d', '\u005f', '\u007e', '\u0024', '\u003b', 
	'\u003f', '\u005a', '\u0061', '\u007a', '\u0030', '\u0039', '\u0041', '\u0046', 
	'\u0061', '\u0066', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', 
	'\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u0030', '\u0039', 
	'\u0041', '\u0046', '\u0061', '\u0066', '\u000d', '\u0020', '\u0021', '\u0023', 
	'\u0025', '\u003d', '\u005f', '\u007e', '\u0024', '\u003b', '\u003f', '\u005a', 
	'\u0061', '\u007a', '\u000d', '\u0020', '\u0021', '\u0023', '\u0025', '\u003d', 
	'\u005f', '\u007e', '\u0024', '\u003b', '\u003f', '\u005a', '\u0061', '\u007a', 
	'\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u0030', '\u0039', 
	'\u0041', '\u0046', '\u0061', '\u0066', '\u000d', '\u0020', '\u0021', '\u0025', 
	'\u002b', '\u003d', '\u0040', '\u005f', '\u007e', '\u0024', '\u002c', '\u002d', 
	'\u002e', '\u0030', '\u0039', '\u003a', '\u003b', '\u0041', '\u005a', '\u0061', 
	'\u007a', '\u000d', '\u0020', '\u0021', '\u0025', '\u002b', '\u003a', '\u003b', 
	'\u003d', '\u0040', '\u005f', '\u007e', '\u0024', '\u002c', '\u002d', '\u002e', 
	'\u0030', '\u0039', '\u0041', '\u005a', '\u0061', '\u007a', '\u000d', '\u0020', 
	'\u0021', '\u0025', '\u003d', '\u005f', '\u007e', '\u0024', '\u003b', '\u003f', 
	'\u005a', '\u0061', '\u007a', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', 
	'\u0066', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u0020', 
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
	'\u0024', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', 
	'\u0020', '\u0024', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', '\u0061', 
	'\u007a', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', '\u0041', 
	'\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', '\u002e', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', 
	'\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0020', '\u0024', 
	'\u0054', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', 
	'\u0020', '\u0024', '\u0054', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u0020', '\u0024', '\u0050', '\u005f', '\u002d', '\u002e', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u002f', '\u005f', 
	'\u002d', '\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u0030', '\u0039', 
	'\u002e', '\u0030', '\u0039', '\u0020', '\u0030', '\u0039', '\u000d', '\u0020', 
	'\u0030', '\u0039', '\u000a', '\u000d', '\u0021', '\u0043', '\u0054', '\u0055', 
	'\u0063', '\u0074', '\u0075', '\u007c', '\u007e', '\u0023', '\u0027', '\u002a', 
	'\u002b', '\u002d', '\u002e', '\u0030', '\u0039', '\u0041', '\u005a', '\u005e', 
	'\u007a', '\u0000', '\u0009', '\u000b', '\u000c', '\u000e', '\u007f', '\u000d', 
	'\u0000', '\u0009', '\u000b', '\u007f', '\u000d', '\u0030', '\u0039', '\u0041', 
	'\u0046', '\u0061', '\u0066', '\u000a', '\u0030', '\u0039', '\u0041', '\u0046', 
	'\u0061', '\u0066', '\u000d', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', 
	'\u0066', '\u000a', (char) 0
};

static readonly sbyte[] _http_parser_single_lengths =  new sbyte [] {
	0, 4, 1, 3, 3, 9, 1, 10, 
	1, 4, 8, 2, 6, 4, 4, 4, 
	4, 4, 2, 4, 4, 4, 2, 4, 
	4, 4, 3, 4, 4, 4, 4, 4, 
	2, 6, 6, 8, 6, 6, 6, 6, 
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
	1, 1, 0, 1, 1, 0, 0, 0, 
	0, 0, 0, 0, 0
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
	3, 0, 3, 3, 0, 0, 0, 0, 
	0, 0, 0, 0, 0
};

static readonly short[] _http_parser_index_offsets =  new short [] {
	0, 0, 8, 10, 17, 24, 37, 39, 
	56, 58, 69, 78, 81, 88, 93, 98, 
	103, 108, 113, 116, 121, 126, 131, 134, 
	139, 144, 149, 153, 158, 163, 168, 173, 
	178, 181, 194, 207, 222, 235, 248, 261, 
	274, 287, 300, 311, 324, 337, 350, 362, 
	375, 388, 401, 414, 427, 440, 451, 464, 
	477, 490, 503, 516, 529, 542, 554, 567, 
	580, 593, 606, 619, 632, 645, 658, 669, 
	682, 695, 708, 721, 734, 747, 758, 760, 
	762, 764, 766, 768, 770, 772, 774, 776, 
	788, 792, 796, 809, 820, 831, 835, 839, 
	843, 847, 859, 871, 875, 879, 895, 912, 
	923, 927, 931, 938, 945, 952, 959, 966, 
	973, 980, 987, 994, 1001, 1008, 1015, 1022, 
	1029, 1036, 1043, 1050, 1057, 1064, 1071, 1078, 
	1085, 1087, 1095, 1103, 1111, 1119, 1121, 1123, 
	1125, 1127, 1129, 1133, 1135, 1152, 1156, 1160, 
	1161, 1166, 1168, 1172, 1177, 1179, 1180, 1181, 
	1182, 1183, 1184, 1185, 1186
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
	25, 27, 28, 27, 28, 24, 1, 30, 
	29, 1, 30, 31, 32, 31, 32, 29, 
	1, 30, 33, 33, 29, 1, 30, 34, 
	34, 29, 1, 30, 35, 35, 29, 1, 
	30, 36, 36, 29, 1, 30, 37, 37, 
	29, 1, 38, 29, 1, 30, 39, 39, 
	29, 1, 30, 40, 40, 29, 1, 30, 
	41, 41, 29, 1, 42, 29, 1, 30, 
	43, 43, 29, 1, 30, 44, 44, 29, 
	1, 30, 45, 45, 29, 1, 30, 46, 
	29, 1, 30, 47, 47, 29, 1, 30, 
	48, 48, 29, 1, 30, 49, 49, 29, 
	1, 30, 50, 50, 29, 1, 30, 51, 
	51, 29, 1, 52, 29, 22, 23, 53, 
	53, 22, 22, 22, 22, 22, 22, 22, 
	22, 1, 22, 23, 54, 54, 22, 22, 
	22, 22, 22, 22, 22, 22, 1, 22, 
	23, 55, 56, 55, 56, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 23, 
	57, 57, 22, 22, 22, 22, 22, 22, 
	22, 22, 1, 22, 23, 58, 58, 22, 
	22, 22, 22, 22, 22, 22, 22, 1, 
	22, 23, 59, 59, 22, 22, 22, 22, 
	22, 22, 22, 22, 1, 22, 23, 60, 
	60, 22, 22, 22, 22, 22, 22, 22, 
	22, 1, 22, 23, 61, 61, 22, 22, 
	22, 22, 22, 22, 22, 22, 1, 22, 
	23, 62, 62, 22, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 63, 22, 22, 
	22, 22, 22, 22, 22, 22, 1, 22, 
	23, 64, 64, 22, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 23, 65, 65, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 66, 66, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 67, 
	22, 23, 22, 22, 22, 22, 22, 22, 
	22, 1, 22, 23, 68, 68, 22, 22, 
	22, 22, 22, 22, 22, 22, 1, 22, 
	23, 69, 69, 22, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 23, 70, 70, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 71, 71, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 23, 
	72, 72, 22, 22, 22, 22, 22, 22, 
	22, 22, 1, 22, 23, 73, 73, 22, 
	22, 22, 22, 22, 22, 22, 22, 1, 
	22, 74, 22, 22, 22, 22, 22, 22, 
	22, 22, 1, 22, 23, 75, 75, 22, 
	22, 22, 22, 22, 22, 22, 22, 1, 
	22, 23, 76, 76, 22, 22, 22, 22, 
	22, 22, 22, 22, 1, 22, 23, 77, 
	77, 22, 22, 22, 22, 22, 22, 22, 
	22, 1, 22, 23, 78, 78, 22, 22, 
	22, 22, 22, 22, 22, 22, 1, 22, 
	23, 79, 79, 22, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 23, 80, 80, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 81, 81, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 82, 
	22, 23, 22, 22, 22, 22, 22, 22, 
	22, 1, 22, 23, 83, 83, 22, 22, 
	22, 22, 22, 22, 22, 22, 1, 22, 
	23, 84, 84, 22, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 23, 85, 85, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 86, 86, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 23, 
	87, 87, 22, 22, 22, 22, 22, 22, 
	22, 22, 1, 22, 23, 88, 88, 22, 
	22, 22, 22, 22, 22, 22, 22, 1, 
	22, 23, 89, 89, 22, 22, 22, 22, 
	22, 22, 22, 22, 1, 22, 23, 90, 
	90, 22, 22, 22, 22, 22, 22, 22, 
	22, 1, 22, 91, 22, 22, 22, 22, 
	22, 22, 22, 22, 1, 22, 23, 92, 
	92, 22, 22, 22, 22, 22, 22, 22, 
	22, 1, 22, 23, 93, 93, 22, 22, 
	22, 22, 22, 22, 22, 22, 1, 22, 
	23, 94, 94, 22, 22, 22, 22, 22, 
	22, 22, 22, 1, 22, 23, 95, 95, 
	22, 22, 22, 22, 22, 22, 22, 22, 
	1, 22, 23, 96, 96, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 22, 23, 
	97, 97, 22, 22, 22, 22, 22, 22, 
	22, 22, 1, 22, 98, 22, 22, 22, 
	22, 22, 22, 22, 22, 1, 99, 1, 
	100, 1, 101, 1, 102, 1, 103, 1, 
	104, 1, 105, 1, 106, 1, 107, 1, 
	108, 109, 110, 111, 110, 110, 110, 110, 
	110, 110, 110, 1, 112, 112, 112, 1, 
	110, 110, 110, 1, 113, 114, 115, 116, 
	117, 115, 118, 115, 115, 115, 115, 115, 
	1, 119, 120, 121, 122, 121, 121, 121, 
	121, 121, 121, 1, 123, 124, 125, 126, 
	125, 125, 125, 125, 125, 125, 1, 127, 
	127, 127, 1, 125, 125, 125, 1, 128, 
	128, 128, 1, 115, 115, 115, 1, 129, 
	130, 131, 132, 133, 131, 131, 131, 131, 
	131, 131, 1, 134, 135, 136, 137, 138, 
	136, 136, 136, 136, 136, 136, 1, 139, 
	139, 139, 1, 136, 136, 136, 1, 108, 
	109, 110, 111, 140, 110, 110, 110, 110, 
	110, 140, 140, 110, 140, 140, 1, 108, 
	109, 110, 111, 140, 141, 110, 110, 110, 
	110, 110, 110, 140, 140, 140, 140, 1, 
	108, 109, 141, 142, 141, 141, 141, 141, 
	141, 141, 1, 143, 143, 143, 1, 141, 
	141, 141, 1, 7, 144, 144, 144, 144, 
	144, 1, 7, 145, 145, 145, 145, 145, 
	1, 7, 146, 146, 146, 146, 146, 1, 
	7, 147, 147, 147, 147, 147, 1, 7, 
	148, 148, 148, 148, 148, 1, 7, 149, 
	149, 149, 149, 149, 1, 7, 150, 150, 
	150, 150, 150, 1, 7, 151, 151, 151, 
	151, 151, 1, 7, 152, 152, 152, 152, 
	152, 1, 7, 153, 153, 153, 153, 153, 
	1, 7, 154, 154, 154, 154, 154, 1, 
	7, 155, 155, 155, 155, 155, 1, 7, 
	156, 156, 156, 156, 156, 1, 7, 157, 
	157, 157, 157, 157, 1, 7, 158, 158, 
	158, 158, 158, 1, 7, 159, 159, 159, 
	159, 159, 1, 7, 160, 160, 160, 160, 
	160, 1, 7, 161, 161, 161, 161, 161, 
	1, 7, 162, 162, 162, 162, 162, 1, 
	7, 163, 163, 163, 163, 163, 1, 7, 
	164, 164, 164, 164, 164, 1, 7, 165, 
	165, 165, 165, 165, 1, 7, 1, 7, 
	8, 166, 8, 8, 8, 8, 1, 7, 
	144, 167, 144, 144, 144, 144, 1, 7, 
	145, 168, 145, 145, 145, 145, 1, 7, 
	146, 169, 146, 146, 146, 146, 1, 170, 
	1, 171, 1, 172, 1, 173, 1, 174, 
	1, 175, 176, 177, 1, 178, 1, 179, 
	180, 181, 182, 183, 181, 182, 183, 180, 
	180, 180, 180, 180, 180, 180, 180, 1, 
	184, 184, 184, 1, 186, 185, 185, 1, 
	187, 188, 189, 189, 189, 1, 190, 1, 
	189, 189, 189, 1, 191, 192, 192, 192, 
	1, 193, 1, 194, 1, 187, 195, 196, 
	197, 198, 1, 0
};

static readonly byte[] _http_parser_trans_targs =  new byte [] {
	2, 0, 4, 129, 3, 4, 129, 5, 
	106, 6, 78, 87, 88, 90, 101, 7, 
	8, 9, 33, 54, 71, 150, 9, 10, 
	11, 10, 6, 12, 23, 11, 6, 13, 
	19, 14, 15, 16, 17, 18, 6, 20, 
	21, 22, 6, 24, 25, 26, 27, 28, 
	29, 30, 31, 32, 6, 34, 35, 36, 
	43, 37, 38, 39, 40, 41, 42, 10, 
	44, 45, 46, 47, 48, 49, 50, 51, 
	52, 53, 10, 55, 56, 57, 58, 59, 
	60, 61, 62, 63, 64, 65, 66, 67, 
	68, 69, 70, 10, 72, 73, 74, 75, 
	76, 77, 10, 79, 80, 81, 82, 83, 
	84, 85, 86, 6, 6, 78, 87, 88, 
	89, 6, 78, 90, 91, 95, 97, 6, 
	78, 92, 93, 6, 78, 92, 93, 94, 
	96, 6, 78, 98, 91, 99, 6, 78, 
	98, 91, 99, 100, 102, 103, 104, 105, 
	107, 108, 109, 110, 111, 112, 113, 114, 
	115, 116, 117, 118, 119, 120, 121, 122, 
	123, 124, 125, 126, 127, 128, 130, 131, 
	132, 133, 134, 135, 136, 137, 138, 139, 
	141, 138, 140, 8, 9, 33, 54, 71, 
	142, 142, 139, 151, 145, 147, 146, 148, 
	147, 153, 156, 152, 154, 154, 155
};

static readonly byte[] _http_parser_trans_actions =  new byte [] {
	53, 0, 117, 117, 9, 41, 41, 56, 
	1, 171, 160, 155, 155, 177, 165, 0, 
	0, 41, 41, 41, 41, 29, 1, 23, 
	41, 0, 44, 41, 41, 1, 25, 1, 
	1, 1, 1, 1, 1, 1, 92, 1, 
	1, 1, 83, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 86, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 80, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 77, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 89, 1, 1, 1, 1, 
	1, 1, 95, 0, 0, 0, 0, 0, 
	19, 0, 21, 15, 59, 11, 1, 1, 
	1, 125, 65, 47, 62, 47, 62, 150, 
	113, 101, 101, 141, 74, 47, 47, 47, 
	47, 145, 109, 101, 105, 101, 129, 71, 
	47, 68, 47, 47, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 0, 19, 0, 21, 5, 121, 41, 
	0, 13, 1, 17, 137, 137, 137, 137, 
	41, 1, 133, 31, 0, 50, 0, 27, 
	3, 0, 0, 37, 98, 35, 35
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
	0, 0, 0, 0, 39
};

static readonly byte[] _http_parser_eof_actions =  new byte [] {
	0, 0, 0, 0, 0, 7, 0, 0, 
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
	0, 7, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	37, 33, 0, 0, 0
};

const int http_parser_start = 1;
const int http_parser_first_final = 150;
const int http_parser_error = 0;

const int http_parser_en_main = 1;
const int http_parser_en_body_identity = 143;
const int http_parser_en_body_identity_eof = 152;
const int http_parser_en_body_chunked_identity = 144;
const int http_parser_en_body_chunked_identity_chunk = 153;
const int http_parser_en_read_chunk_stop = 155;
const int http_parser_en_dead = 149;


#line 454 "HttpParser2-chunked.cs.rl"

    /// <summary>Initializes the state machine. Use the delegate-taking constructor instead.</summary>
    protected HttpCombinedParser()
    {
			_stringBuilder = new StringBuilder();
        
#line 642 "../HttpCombinedParser.cs"
	{
	cs = http_parser_start;
	}

#line 460 "HttpParser2-chunked.cs.rl"
    }

    /// <summary>
    /// Creates a parser that reports its results to <paramref name="del"/>. When the delegate
    /// also implements <see cref="IHttpParserSpanDelegate"/>, body data from span input is
    /// delivered without copying.
    /// </summary>
    public HttpCombinedParser(IHttpParserCombinedDelegate del) : this()
    {
        this.parserDelegate = del;
        this.spanDelegate = del as IHttpParserSpanDelegate;
    }

		private static int HexValue(char c) => c <= '9' ? c - '0' : (c | 0x20) - 'a' + 10;

		// Body callbacks mirror the Execute overload that was used: array-based input is
		// delivered as an ArraySegment over the caller's buffer (as in 5.x); span-based
		// input goes to IHttpParserSpanDelegate when implemented, or to the ArraySegment
		// callback via a pooled copy otherwise.
		private void EmitBody(ReadOnlySpan<byte> data, byte[] array, int arrayOffset, int start, int count)
		{
			if (array != null)
			{
				parserDelegate.OnBody(this, new ArraySegment<byte>(array, arrayOffset + start, count));
			}
			else if (spanDelegate != null)
			{
				spanDelegate.OnBody(this, data.Slice(start, count));
			}
			else
			{
				var rented = ArrayPool<byte>.Shared.Rent(count);
				try
				{
					data.Slice(start, count).CopyTo(rented);
					parserDelegate.OnBody(this, new ArraySegment<byte>(rented, 0, count));
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(rented);
				}
			}
		}

		/// <summary>
		/// Parses the entire contents of <paramref name="buff"/> (independent of its current
		/// position). The stream is not copied when its buffer is exposable.
		/// Returns the number of bytes consumed; less than the stream length means a parse
		/// error at that offset.
		/// </summary>
		public int Execute(MemoryStream buff) =>
			buff.TryGetBuffer(out var buffer)
				? Execute(new ReadOnlySpan<byte>(buffer.Array, buffer.Offset, buffer.Count), buffer.Array, buffer.Offset)
				: Execute(buff.ToArray());

    /// <summary>
    /// Parses <paramref name="buff"/>. Call repeatedly as data arrives; messages may be split
    /// across calls at any byte. Pass an empty buffer to signal end of stream for bodies
    /// delimited by connection close. Returns the number of bytes consumed; less than
    /// <c>buff.Length</c> means a parse error at that offset.
    /// </summary>
    public int Execute(byte[] buff) => Execute(new ReadOnlySpan<byte>(buff), buff, 0);

    /// <summary>
    /// Parses the given segment. Call repeatedly as data arrives; messages may be split
    /// across calls at any byte. Pass an empty segment to signal end of stream for bodies
    /// delimited by connection close. Returns the number of bytes consumed; less than
    /// <c>buf.Count</c> means a parse error at that offset.
    /// </summary>
    public int Execute(ArraySegment<byte> buf) => Execute(buf.AsSpan(), buf.Array, buf.Offset);

    /// <summary>
    /// Parses the given span without requiring an array. Body data is delivered via
    /// <see cref="IHttpParserSpanDelegate.OnBody"/> when the delegate implements it, or as a
    /// pooled copy through <see cref="IHttpParserDelegate.OnBody"/> otherwise. Pass an empty
    /// span to signal end of stream for bodies delimited by connection close. Returns the
    /// number of bytes consumed; less than <c>buf.Length</c> means a parse error at that offset.
    /// </summary>
    public int Execute(ReadOnlySpan<byte> buf) => Execute(buf, null, 0);

    private int Execute(ReadOnlySpan<byte> data, byte[] array, int arrayOffset)
    {
			int p = 0;
			int pe = data.Length;
			int eof = data.Length == 0 ? 0 : -1;

			try
			{
				
#line 733 "../HttpCombinedParser.cs"
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
#line 445 "HttpParser2-chunked.cs.rl"
	{
			throw new Exception("Parser is dead; there shouldn't be more data. Client is bogus? fpc =" + p);
		}
	break;
#line 754 "../HttpCombinedParser.cs"
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
#line 64 "HttpParser2-chunked.cs.rl"
	{
			_stringBuilder.Append((char)data[p]);
		}
	break;
	case 1:
#line 68 "HttpParser2-chunked.cs.rl"
	{
			_stringBuilder.Length = 0;
		}
	break;
	case 2:
#line 72 "HttpParser2-chunked.cs.rl"
	{
			_stringBuilder2.Append((char)data[p]);
		}
	break;
	case 3:
#line 76 "HttpParser2-chunked.cs.rl"
	{
			if (_stringBuilder2 == null)
				_stringBuilder2 = new StringBuilder();
		 	_stringBuilder2.Length = 0;
		}
	break;
	case 4:
#line 82 "HttpParser2-chunked.cs.rl"
	{
			_chunkLength = (_chunkLength << 4) | HexValue((char)data[p]);
		}
	break;
	case 5:
#line 86 "HttpParser2-chunked.cs.rl"
	{
			_chunkLength = 0;
		}
	break;
	case 6:
#line 90 "HttpParser2-chunked.cs.rl"
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
			_isRequest = false;
			parserDelegate.OnMessageBegin(this);
		}
	break;
	case 7:
#line 107 "HttpParser2-chunked.cs.rl"
	{
           //Console.WriteLine("matched absolute_uri");
        }
	break;
	case 8:
#line 110 "HttpParser2-chunked.cs.rl"
	{
            //Console.WriteLine("matched abs_path");
        }
	break;
	case 9:
#line 113 "HttpParser2-chunked.cs.rl"
	{
            //Console.WriteLine("matched authority");
        }
	break;
	case 10:
#line 116 "HttpParser2-chunked.cs.rl"
	{
            //Console.WriteLine("matched first space");
        }
	break;
	case 11:
#line 119 "HttpParser2-chunked.cs.rl"
	{
            //Console.WriteLine("leave_first_space");
        }
	break;
	case 13:
#line 128 "HttpParser2-chunked.cs.rl"
	{
			//Console.WriteLine("matched_leading_crlf");
		}
	break;
	case 14:
#line 138 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnMethod(this, _stringBuilder.ToString());
		}
	break;
	case 15:
#line 142 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnRequestUri(this, _stringBuilder.ToString());
		}
	break;
	case 16:
#line 147 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnPath(this, _stringBuilder2.ToString());
		}
	break;
	case 17:
#line 152 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnQueryString(this, _stringBuilder2.ToString());
		}
	break;
	case 18:
#line 157 "HttpParser2-chunked.cs.rl"
	{
			statusCode = 0;
		}
	break;
	case 19:
#line 162 "HttpParser2-chunked.cs.rl"
	{
			statusCode = statusCode * 10 + ((char)data[p] - '0');
		}
	break;
	case 20:
#line 167 "HttpParser2-chunked.cs.rl"
	{
			statusReason = _stringBuilder.ToString();
		}
	break;
	case 21:
#line 172 "HttpParser2-chunked.cs.rl"
	{
			_isRequest = true;
			parserDelegate.OnRequestType(this);
		}
	break;
	case 22:
#line 178 "HttpParser2-chunked.cs.rl"
	{
			_isRequest = false;
			parserDelegate.OnResponseType(this);
			parserDelegate.OnResponseCode(this, statusCode, statusReason);
			statusReason = null;
			statusCode = 0;
		}
	break;
	case 23:
#line 196 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnFragment(this, _stringBuilder2.ToString());
		}
	break;
	case 24:
#line 209 "HttpParser2-chunked.cs.rl"
	{
			MajorVersion = (char)data[p] - '0';
		}
	break;
	case 25:
#line 213 "HttpParser2-chunked.cs.rl"
	{
			MinorVersion = (char)data[p] - '0';
		}
	break;
	case 26:
#line 217 "HttpParser2-chunked.cs.rl"
	{
            if (_contentLength != -1) throw new Exception("Already got Content-Length. Possible attack?");
			//Console.WriteLine("Saw content length");
			_contentLength = 0;
			inContentLengthHeader = true;
        }
	break;
	case 27:
#line 224 "HttpParser2-chunked.cs.rl"
	{
			//Console.WriteLine("header_connection");
			inConnectionHeader = true;
		}
	break;
	case 28:
#line 229 "HttpParser2-chunked.cs.rl"
	{
			//Console.WriteLine("header_connection_close");
			if (inConnectionHeader)
				gotConnectionClose = true;
		}
	break;
	case 29:
#line 235 "HttpParser2-chunked.cs.rl"
	{
			//Console.WriteLine("header_connection_keepalive");
			if (inConnectionHeader)
				gotConnectionKeepAlive = true;
		}
	break;
	case 30:
#line 241 "HttpParser2-chunked.cs.rl"
	{
			//Console.WriteLine("Saw transfer encoding");
			inTransferEncodingHeader = true;
		}
	break;
	case 31:
#line 246 "HttpParser2-chunked.cs.rl"
	{
			if (inTransferEncodingHeader)
				gotTransferEncodingChunked = true;
            parserDelegate.OnTransferEncodingChunked(this, true);
			Debug.WriteLine($"Transfer Encoding Chunked: {gotTransferEncodingChunked}");
		}
	break;
	case 32:
#line 253 "HttpParser2-chunked.cs.rl"
	{
			inUpgradeHeader = true;
		}
	break;
	case 33:
#line 257 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnHeaderName(this, _stringBuilder.ToString());
		}
	break;
	case 34:
#line 261 "HttpParser2-chunked.cs.rl"
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
	case 35:
#line 273 "HttpParser2-chunked.cs.rl"
	{
			Debug.WriteLine($"Chunk Length: {_chunkLength}");
			parserDelegate.OnChunkedLength(this, _chunkLength);
        }
	break;
	case 36:
#line 278 "HttpParser2-chunked.cs.rl"
	{
			
			if (data[p] == 10)
			{
				//Console.WriteLine("leave_headers contentLength = " + contentLength);
				parserDelegate.OnHeadersEnd(this);

				// if chunked transfer, ignore content length and parse chunked
				// if content length given but zero, read next request
				// if content length is given and non-zero, we should read that many bytes
				// if content length is not given
				//   if this is a request, the body length is zero
				//   else if should keep alive, assume next response is coming and read it
				//   else read the body until the connection closes (EOF)

				if (gotTransferEncodingChunked)
				{
					// RFC 9112 6.3: Transfer-Encoding takes precedence over Content-Length
					{cs = 144;if (true) goto _again;}
				}
				else if (_contentLength == 0)
				{
					// No Content. Get ready for new incoming request
					parserDelegate.OnMessageEnd(this);
					{cs = 1;if (true) goto _again;}
				}
				else if (_contentLength > 0)
				{
					// Handle Body based on Content Length
					{cs = 143;if (true) goto _again;}
				}
				else if (_isRequest)
				{
					// RFC 9112 6.3 rule 6: a request with neither Transfer-Encoding nor
					// Content-Length has a body length of zero. Close-delimited bodies
					// are a response-only rule.
					parserDelegate.OnMessageEnd(this);
					{cs = 1;if (true) goto _again;}
				}
				else
				{
					if (ShouldKeepAlive)
					{
						parserDelegate.OnMessageEnd(this);
						{cs = 1;if (true) goto _again;}
					}
					else
					{
						// No framing information: the body runs until the connection closes.
						// Signal EOF by calling Execute with an empty buffer.
						{cs = 152;if (true) goto _again;}
					}
				}
			}
        }
	break;
	case 37:
#line 334 "HttpParser2-chunked.cs.rl"
	{
			var toRead = Math.Min(pe - p, _contentLength);
			//Console.WriteLine("body_identity: reading " + toRead + " bytes from body.");
			if (toRead > 0)
			{
				EmitBody(data, array, arrayOffset, p, toRead);
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
						{cs = 1;if (true) goto _again;}
					}
					else
					{
						//fhold;
						{cs = 149;if (true) goto _again;}
					}
				}
				else
				{
					{p++; if (true) goto _out; }
				}
			}
		}
	break;
	case 38:
#line 367 "HttpParser2-chunked.cs.rl"
	{
			Debug.WriteLine($"Reading chunk size: {_chunkLength}.");
			var toRead = Math.Min(pe - p, _chunkLength);
			if (toRead > 0)
			{
				Debug.WriteLine($"To Read: {toRead}");
				parserDelegate.OnChunkReceived(this);
				EmitBody(data, array, arrayOffset, p, toRead);
				p += toRead - 1;
				_chunkLength -= toRead;
				
				if (_chunkLength == 0)
				{
					// Finished reading the current chunk. Go to the next one.
					{cs = 144;if (true) goto _again;}
				}
				else
				{
					// Additional chunk data will be present in the next buffer. Stay in the same sate.
					{cs = 153;if (true) goto _again;}
				}
			}

			if (_chunkLength == 0)
			{
				{cs = 155;if (true) goto _again;}
			}
			else
			{
				{p++; if (true) goto _out; }
			}
		}
	break;
	case 39:
#line 400 "HttpParser2-chunked.cs.rl"
	{
			//Debug.WriteLine($"End of chunks");

			parserDelegate.OnMessageEnd(this);
			
			if (ShouldKeepAlive)
			{
				{cs = 1;if (true) goto _again;}
			}
			else
			{
				p--;
				{cs = 149;if (true) goto _again;}
			}
		}
	break;
	case 40:
#line 416 "HttpParser2-chunked.cs.rl"
	{
			var toRead = pe - p;
			Debug.WriteLine($"Eof To Read: {toRead}");
			if (toRead > 0)
			{
				EmitBody(data, array, arrayOffset, p, toRead);
				p += toRead - 1;
				{p++; if (true) goto _out; }
			}
			else
			{
				// Reached only from the EOF action (empty Execute call).
				parserDelegate.OnMessageEnd(this);

				if (ShouldKeepAlive)
				{
					p--;
					{cs = 1;if (true) goto _again;}
				}
				else
				{
					p--;
					{cs = 149;if (true) goto _again;}
				}
			}
		}
	break;
#line 1205 "../HttpCombinedParser.cs"
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
	case 12:
#line 122 "HttpParser2-chunked.cs.rl"
	{
            //Console.WriteLine("eof_leave_first_space");
        }
	break;
	case 38:
#line 367 "HttpParser2-chunked.cs.rl"
	{
			Debug.WriteLine($"Reading chunk size: {_chunkLength}.");
			var toRead = Math.Min(pe - p, _chunkLength);
			if (toRead > 0)
			{
				Debug.WriteLine($"To Read: {toRead}");
				parserDelegate.OnChunkReceived(this);
				EmitBody(data, array, arrayOffset, p, toRead);
				p += toRead - 1;
				_chunkLength -= toRead;
				
				if (_chunkLength == 0)
				{
					// Finished reading the current chunk. Go to the next one.
					{cs = 144;if (true) goto _again;}
				}
				else
				{
					// Additional chunk data will be present in the next buffer. Stay in the same sate.
					{cs = 153;if (true) goto _again;}
				}
			}

			if (_chunkLength == 0)
			{
				{cs = 155;if (true) goto _again;}
			}
			else
			{
				{p++; if (true) goto _out; }
			}
		}
	break;
	case 40:
#line 416 "HttpParser2-chunked.cs.rl"
	{
			var toRead = pe - p;
			Debug.WriteLine($"Eof To Read: {toRead}");
			if (toRead > 0)
			{
				EmitBody(data, array, arrayOffset, p, toRead);
				p += toRead - 1;
				{p++; if (true) goto _out; }
			}
			else
			{
				// Reached only from the EOF action (empty Execute call).
				parserDelegate.OnMessageEnd(this);

				if (ShouldKeepAlive)
				{
					p--;
					{cs = 1;if (true) goto _again;}
				}
				else
				{
					p--;
					{cs = 149;if (true) goto _again;}
				}
			}
		}
	break;
#line 1288 "../HttpCombinedParser.cs"
		default: break;
		}
	}
	}

	_out: {}
	}

#line 549 "HttpParser2-chunked.cs.rl"
			}
			catch (Exception)
			{
            parserDelegate.OnParserError();
			}

			if (p >= 0 && p < data.Length)
			{
				Debug.WriteLine("error on character " + p);
				Debug.WriteLine("('" + (char)data[p] + "')");
			}

			return p;
    }
}