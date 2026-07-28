
#line 1 "HttpParser2-chunked.cs.rl"
﻿using System;
using System.Buffers;
using System.Text;
using System.Diagnostics;
using HttpMachine.Headers;
using IHttpMachine;
using System.IO;

namespace HttpMachine;

/// <summary>
/// Combined HTTP request/response parser built on a Ragel-generated state machine.
/// Feed it data with the <c>Execute</c> overloads; results arrive through the callbacks on
/// the <see cref="IHttpParserCombinedDelegate"/> supplied to the constructor.
/// This class is generated from <c>rl/HttpParser2-chunked.cs.rl</c> - edit that file, not the .cs.
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
    private readonly UnframedResponseMode _unframedResponseMode;
    private readonly HeaderNameMode _headerNameMode;

		private readonly StringBuilder _stringBuilder;
		private StringBuilder _stringBuilder2;

		// Header names are accumulated here as raw bytes instead of into _stringBuilder, so the
		// completed name can be matched as a span without materialising a string first. The char
		// scratch is only used for names the matcher does not recognise. Both grow together; see
		// GrowHeaderNameBuffer.
		private byte[] _headerNameBytes = new byte[64];
		private char[] _headerNameChars = new char[64];
		private int _headerNameLength;

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

    
#line 460 "HttpParser2-chunked.cs.rl"


    
#line 73 "../HttpCombinedParser.cs"
static readonly sbyte[] _http_parser_actions =  new sbyte [] {
	0, 1, 0, 1, 4, 1, 10, 1, 
	12, 1, 13, 1, 15, 1, 19, 1, 
	21, 1, 22, 1, 24, 1, 25, 1, 
	30, 1, 31, 1, 32, 1, 33, 1, 
	34, 1, 35, 1, 36, 1, 37, 1, 
	38, 1, 39, 2, 1, 0, 2, 1, 
	32, 2, 2, 0, 2, 5, 4, 2, 
	6, 13, 2, 14, 10, 2, 15, 21, 
	2, 16, 0, 2, 16, 15, 2, 17, 
	0, 2, 17, 15, 2, 23, 15, 2, 
	26, 32, 2, 27, 32, 2, 28, 32, 
	2, 29, 30, 2, 36, 37, 3, 3, 
	2, 0, 3, 3, 17, 0, 3, 3, 
	17, 15, 3, 3, 23, 15, 3, 6, 
	1, 0, 3, 11, 18, 19, 3, 16, 
	15, 21, 3, 17, 15, 21, 3, 20, 
	1, 0, 3, 22, 29, 30, 3, 23, 
	15, 21, 4, 3, 17, 15, 21, 4, 
	3, 23, 15, 21, 4, 11, 1, 9, 
	0, 4, 11, 1, 9, 15, 5, 11, 
	1, 7, 9, 0, 5, 11, 1, 9, 
	15, 21, 6, 11, 1, 8, 3, 2, 
	0
};

static readonly short[] _http_parser_key_offsets =  new short [] {
	0, 0, 10, 11, 20, 29, 44, 45, 
	61, 62, 78, 86, 88, 94, 98, 102, 
	106, 110, 114, 116, 120, 124, 128, 130, 
	134, 138, 142, 145, 149, 153, 157, 161, 
	165, 167, 168, 169, 170, 171, 172, 174, 
	175, 177, 178, 193, 199, 205, 220, 233, 
	246, 252, 258, 264, 270, 284, 298, 304, 
	310, 331, 352, 365, 371, 377, 386, 395, 
	404, 413, 422, 431, 440, 449, 458, 467, 
	476, 485, 494, 503, 512, 521, 530, 539, 
	548, 557, 566, 575, 576, 586, 596, 606, 
	616, 618, 619, 621, 622, 624, 628, 629, 
	645, 651, 656, 656, 663, 664, 670, 677, 
	678, 678, 678, 678, 678, 678, 678, 678
};

static readonly char[] _http_parser_trans_keys =  new char [] {
	'\u000d', '\u0024', '\u0048', '\u005f', '\u002d', '\u002e', '\u0041', '\u005a', 
	'\u0061', '\u007a', '\u000a', '\u0024', '\u0048', '\u005f', '\u002d', '\u002e', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u0020', '\u0024', '\u005f', '\u002d', 
	'\u002e', '\u0041', '\u005a', '\u0061', '\u007a', '\u000d', '\u0020', '\u0021', 
	'\u0025', '\u002f', '\u003d', '\u0040', '\u005f', '\u007e', '\u0024', '\u003b', 
	'\u0041', '\u005a', '\u0061', '\u007a', '\u000a', '\u000d', '\u0021', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u000a', '\u0021', '\u003a', 
	'\u007c', '\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', 
	'\u0030', '\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0009', '\u000a', 
	'\u000d', '\u0020', '\u0043', '\u004b', '\u0063', '\u006b', '\u000a', '\u000d', 
	'\u000a', '\u000d', '\u0048', '\u004c', '\u0068', '\u006c', '\u000a', '\u000d', 
	'\u0055', '\u0075', '\u000a', '\u000d', '\u004e', '\u006e', '\u000a', '\u000d', 
	'\u004b', '\u006b', '\u000a', '\u000d', '\u0045', '\u0065', '\u000a', '\u000d', 
	'\u0044', '\u0064', '\u000a', '\u000d', '\u000a', '\u000d', '\u004f', '\u006f', 
	'\u000a', '\u000d', '\u0053', '\u0073', '\u000a', '\u000d', '\u0045', '\u0065', 
	'\u000a', '\u000d', '\u000a', '\u000d', '\u0045', '\u0065', '\u000a', '\u000d', 
	'\u0045', '\u0065', '\u000a', '\u000d', '\u0050', '\u0070', '\u000a', '\u000d', 
	'\u002d', '\u000a', '\u000d', '\u0041', '\u0061', '\u000a', '\u000d', '\u004c', 
	'\u006c', '\u000a', '\u000d', '\u0049', '\u0069', '\u000a', '\u000d', '\u0056', 
	'\u0076', '\u000a', '\u000d', '\u0045', '\u0065', '\u000a', '\u000d', '\u0048', 
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
	'\u000d', '\u0020', '\u0030', '\u0039', '\u000a', '\u000d', '\u0021', '\u007c', 
	'\u007e', '\u0023', '\u0027', '\u002a', '\u002b', '\u002d', '\u002e', '\u0030', 
	'\u0039', '\u0041', '\u005a', '\u005e', '\u007a', '\u0000', '\u0009', '\u000b', 
	'\u000c', '\u000e', '\u007f', '\u000d', '\u0000', '\u0009', '\u000b', '\u007f', 
	'\u000d', '\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000a', 
	'\u0030', '\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000d', '\u0030', 
	'\u0039', '\u0041', '\u0046', '\u0061', '\u0066', '\u000a', (char) 0
};

static readonly sbyte[] _http_parser_single_lengths =  new sbyte [] {
	0, 4, 1, 3, 3, 9, 1, 4, 
	1, 4, 8, 2, 6, 4, 4, 4, 
	4, 4, 2, 4, 4, 4, 2, 4, 
	4, 4, 3, 4, 4, 4, 4, 4, 
	2, 1, 1, 1, 1, 1, 0, 1, 
	0, 1, 7, 0, 0, 9, 7, 7, 
	0, 0, 0, 0, 8, 8, 0, 0, 
	9, 11, 7, 0, 0, 3, 3, 3, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	3, 3, 3, 1, 4, 4, 4, 4, 
	0, 1, 0, 1, 0, 2, 1, 4, 
	0, 1, 0, 1, 1, 0, 1, 1, 
	0, 0, 0, 0, 0, 0, 0, 0
};

static readonly sbyte[] _http_parser_range_lengths =  new sbyte [] {
	0, 3, 0, 3, 3, 3, 0, 6, 
	0, 6, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 1, 0, 
	1, 0, 4, 3, 3, 3, 3, 3, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	6, 5, 3, 3, 3, 3, 3, 3, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	3, 3, 3, 3, 3, 3, 3, 3, 
	3, 3, 3, 0, 3, 3, 3, 3, 
	1, 0, 1, 0, 1, 1, 0, 6, 
	3, 2, 0, 3, 0, 3, 3, 0, 
	0, 0, 0, 0, 0, 0, 0, 0
};

static readonly short[] _http_parser_index_offsets =  new short [] {
	0, 0, 8, 10, 17, 24, 37, 39, 
	50, 52, 63, 72, 75, 82, 87, 92, 
	97, 102, 107, 110, 115, 120, 125, 128, 
	133, 138, 143, 147, 152, 157, 162, 167, 
	172, 175, 177, 179, 181, 183, 185, 187, 
	189, 191, 193, 205, 209, 213, 226, 237, 
	248, 252, 256, 260, 264, 276, 288, 292, 
	296, 312, 329, 340, 344, 348, 355, 362, 
	369, 376, 383, 390, 397, 404, 411, 418, 
	425, 432, 439, 446, 453, 460, 467, 474, 
	481, 488, 495, 502, 504, 512, 520, 528, 
	536, 538, 540, 542, 544, 546, 550, 552, 
	563, 567, 571, 572, 577, 579, 583, 588, 
	590, 591, 592, 593, 594, 595, 596, 597
};

static readonly byte[] _http_parser_indicies =  new byte [] {
	0, 2, 3, 2, 2, 2, 2, 1, 
	4, 1, 5, 6, 5, 5, 5, 5, 
	1, 7, 8, 8, 8, 8, 8, 1, 
	9, 10, 11, 12, 13, 11, 11, 11, 
	11, 11, 14, 14, 1, 15, 1, 16, 
	17, 17, 17, 17, 17, 17, 17, 17, 
	17, 1, 18, 1, 19, 20, 19, 19, 
	19, 19, 19, 19, 19, 19, 1, 22, 
	1, 23, 22, 24, 25, 24, 25, 21, 
	1, 27, 26, 1, 27, 28, 29, 28, 
	29, 26, 1, 27, 30, 30, 26, 1, 
	27, 31, 31, 26, 1, 27, 32, 32, 
	26, 1, 27, 33, 33, 26, 1, 27, 
	34, 34, 26, 1, 35, 26, 1, 27, 
	36, 36, 26, 1, 27, 37, 37, 26, 
	1, 27, 38, 38, 26, 1, 39, 26, 
	1, 27, 40, 40, 26, 1, 27, 41, 
	41, 26, 1, 27, 42, 42, 26, 1, 
	27, 43, 26, 1, 27, 44, 44, 26, 
	1, 27, 45, 45, 26, 1, 27, 46, 
	46, 26, 1, 27, 47, 47, 26, 1, 
	27, 48, 48, 26, 1, 49, 26, 50, 
	1, 51, 1, 52, 1, 53, 1, 54, 
	1, 55, 1, 56, 1, 57, 1, 58, 
	1, 59, 60, 61, 62, 61, 61, 61, 
	61, 61, 61, 61, 1, 63, 63, 63, 
	1, 61, 61, 61, 1, 64, 65, 66, 
	67, 68, 66, 69, 66, 66, 66, 66, 
	66, 1, 70, 71, 72, 73, 72, 72, 
	72, 72, 72, 72, 1, 74, 75, 76, 
	77, 76, 76, 76, 76, 76, 76, 1, 
	78, 78, 78, 1, 76, 76, 76, 1, 
	79, 79, 79, 1, 66, 66, 66, 1, 
	80, 81, 82, 83, 84, 82, 82, 82, 
	82, 82, 82, 1, 85, 86, 87, 88, 
	89, 87, 87, 87, 87, 87, 87, 1, 
	90, 90, 90, 1, 87, 87, 87, 1, 
	59, 60, 61, 62, 91, 61, 61, 61, 
	61, 61, 91, 91, 61, 91, 91, 1, 
	59, 60, 61, 62, 91, 92, 61, 61, 
	61, 61, 61, 61, 91, 91, 91, 91, 
	1, 59, 60, 92, 93, 92, 92, 92, 
	92, 92, 92, 1, 94, 94, 94, 1, 
	92, 92, 92, 1, 7, 95, 95, 95, 
	95, 95, 1, 7, 96, 96, 96, 96, 
	96, 1, 7, 97, 97, 97, 97, 97, 
	1, 7, 98, 98, 98, 98, 98, 1, 
	7, 99, 99, 99, 99, 99, 1, 7, 
	100, 100, 100, 100, 100, 1, 7, 101, 
	101, 101, 101, 101, 1, 7, 102, 102, 
	102, 102, 102, 1, 7, 103, 103, 103, 
	103, 103, 1, 7, 104, 104, 104, 104, 
	104, 1, 7, 105, 105, 105, 105, 105, 
	1, 7, 106, 106, 106, 106, 106, 1, 
	7, 107, 107, 107, 107, 107, 1, 7, 
	108, 108, 108, 108, 108, 1, 7, 109, 
	109, 109, 109, 109, 1, 7, 110, 110, 
	110, 110, 110, 1, 7, 111, 111, 111, 
	111, 111, 1, 7, 112, 112, 112, 112, 
	112, 1, 7, 113, 113, 113, 113, 113, 
	1, 7, 114, 114, 114, 114, 114, 1, 
	7, 115, 115, 115, 115, 115, 1, 7, 
	116, 116, 116, 116, 116, 1, 7, 1, 
	7, 8, 117, 8, 8, 8, 8, 1, 
	7, 95, 118, 95, 95, 95, 95, 1, 
	7, 96, 119, 96, 96, 96, 96, 1, 
	7, 97, 120, 97, 97, 97, 97, 1, 
	121, 1, 122, 1, 123, 1, 124, 1, 
	125, 1, 126, 127, 128, 1, 129, 1, 
	130, 131, 131, 131, 131, 131, 131, 131, 
	131, 131, 1, 132, 132, 132, 1, 134, 
	133, 133, 1, 135, 136, 137, 137, 137, 
	1, 138, 1, 137, 137, 137, 1, 139, 
	140, 140, 140, 1, 141, 1, 142, 1, 
	135, 143, 144, 145, 146, 1, 0
};

static readonly byte[] _http_parser_trans_targs =  new byte [] {
	2, 0, 4, 84, 3, 4, 84, 5, 
	61, 6, 33, 42, 43, 45, 56, 7, 
	8, 9, 105, 9, 10, 11, 10, 6, 
	12, 23, 11, 6, 13, 19, 14, 15, 
	16, 17, 18, 6, 20, 21, 22, 6, 
	24, 25, 26, 27, 28, 29, 30, 31, 
	32, 6, 34, 35, 36, 37, 38, 39, 
	40, 41, 6, 6, 33, 42, 43, 44, 
	6, 33, 45, 46, 50, 52, 6, 33, 
	47, 48, 6, 33, 47, 48, 49, 51, 
	6, 33, 53, 46, 54, 6, 33, 53, 
	46, 54, 55, 57, 58, 59, 60, 62, 
	63, 64, 65, 66, 67, 68, 69, 70, 
	71, 72, 73, 74, 75, 76, 77, 78, 
	79, 80, 81, 82, 83, 85, 86, 87, 
	88, 89, 90, 91, 92, 93, 94, 96, 
	93, 95, 8, 9, 97, 97, 94, 106, 
	100, 102, 101, 103, 102, 108, 111, 107, 
	109, 109, 110
};

static readonly byte[] _http_parser_trans_actions =  new byte [] {
	55, 0, 110, 110, 9, 43, 43, 58, 
	1, 164, 153, 148, 148, 170, 158, 0, 
	0, 88, 31, 23, 25, 43, 0, 46, 
	43, 43, 1, 27, 1, 1, 1, 1, 
	1, 1, 1, 85, 1, 1, 1, 79, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 82, 0, 0, 0, 0, 0, 19, 
	0, 21, 15, 61, 11, 1, 1, 1, 
	118, 67, 49, 64, 49, 64, 143, 106, 
	94, 94, 134, 76, 49, 49, 49, 49, 
	138, 102, 94, 98, 94, 122, 73, 49, 
	70, 49, 49, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	1, 1, 1, 1, 1, 1, 1, 1, 
	0, 19, 0, 21, 5, 114, 43, 0, 
	13, 1, 17, 130, 43, 1, 126, 33, 
	0, 52, 0, 29, 3, 0, 0, 39, 
	91, 37, 37
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
	0, 0, 0, 0, 0, 0, 0, 41
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
	0, 0, 0, 0, 7, 0, 0, 0, 
	0, 0, 0, 0, 0, 0, 0, 0, 
	0, 0, 0, 39, 35, 0, 0, 0
};

const int http_parser_start = 1;
const int http_parser_first_final = 105;
const int http_parser_error = 0;

const int http_parser_en_main = 1;
const int http_parser_en_body_identity = 98;
const int http_parser_en_body_identity_eof = 107;
const int http_parser_en_body_chunked_identity = 99;
const int http_parser_en_body_chunked_identity_chunk = 108;
const int http_parser_en_read_chunk_stop = 110;
const int http_parser_en_dead = 104;


#line 463 "HttpParser2-chunked.cs.rl"

    /// <summary>Initializes the state machine. Use the delegate-taking constructor instead.</summary>
    protected HttpCombinedParser()
    {
			_stringBuilder = new StringBuilder();
        
#line 428 "../HttpCombinedParser.cs"
	{
	cs = http_parser_start;
	}

#line 469 "HttpParser2-chunked.cs.rl"
    }

    /// <summary>
    /// Creates a parser that reports its results to <paramref name="del"/>. When the delegate
    /// also implements <see cref="IHttpParserSpanDelegate"/>, body data from span input is
    /// delivered without copying. Uses <see cref="UnframedResponseMode.CompleteAtHeaders"/> and
    /// <see cref="HeaderNameMode.AsReceived"/>.
    /// </summary>
    public HttpCombinedParser(IHttpParserCombinedDelegate del)
        : this(del, UnframedResponseMode.CompleteAtHeaders, HeaderNameMode.AsReceived)
    {
    }

    /// <summary>
    /// Creates a parser that reports its results to <paramref name="del"/>, choosing how responses
    /// with no framing headers are delimited via <paramref name="unframedResponseMode"/>. When the
    /// delegate also implements <see cref="IHttpParserSpanDelegate"/>, body data from span input is
    /// delivered without copying. Uses <see cref="HeaderNameMode.AsReceived"/>.
    /// </summary>
    public HttpCombinedParser(IHttpParserCombinedDelegate del, UnframedResponseMode unframedResponseMode)
        : this(del, unframedResponseMode, HeaderNameMode.AsReceived)
    {
    }

    /// <summary>
    /// Creates a parser that reports its results to <paramref name="del"/>, choosing which spelling
    /// of a recognised header name is reported via <paramref name="headerNameMode"/>. When the
    /// delegate also implements <see cref="IHttpParserSpanDelegate"/>, body data from span input is
    /// delivered without copying. Uses <see cref="UnframedResponseMode.CompleteAtHeaders"/>.
    /// </summary>
    public HttpCombinedParser(IHttpParserCombinedDelegate del, HeaderNameMode headerNameMode)
        : this(del, UnframedResponseMode.CompleteAtHeaders, headerNameMode)
    {
    }

    // Two independent options already need four constructors. If a third is ever added, replace
    // these with an options type rather than continuing to double the overload count.

    /// <summary>
    /// Creates a parser that reports its results to <paramref name="del"/>, choosing both how
    /// responses with no framing headers are delimited and which spelling of a recognised header
    /// name is reported. When the delegate also implements <see cref="IHttpParserSpanDelegate"/>,
    /// body data from span input is delivered without copying.
    /// </summary>
    public HttpCombinedParser(
        IHttpParserCombinedDelegate del,
        UnframedResponseMode unframedResponseMode,
        HeaderNameMode headerNameMode) : this()
    {
        this.parserDelegate = del;
        this.spanDelegate = del as IHttpParserSpanDelegate;
        this._unframedResponseMode = unframedResponseMode;
        this._headerNameMode = headerNameMode;
    }

		private static int HexValue(char c) => c <= '9' ? c - '0' : (c | 0x20) - 'a' + 10;

		private void GrowHeaderNameBuffer()
		{
			Array.Resize(ref _headerNameBytes, _headerNameBytes.Length * 2);
			_headerNameChars = new char[_headerNameBytes.Length];
		}

		/// <summary>
		/// Resolves the accumulated header-name bytes to the string reported to the delegate, and
		/// applies any framing meaning the name carries.
		/// </summary>
		private string ResolveHeaderName()
		{
			var name = new ReadOnlySpan<byte>(_headerNameBytes, 0, _headerNameLength);

			if (KnownHeaders.TryMatch(name, out var known, out var canonical, out var exactCasing))
			{
				ApplyKnownHeader(known);

				// The cached instance stands in for the received bytes when it is byte-identical to
				// them - the string content is the same either way, so this is not observable - or
				// when the caller asked for canonical names.
				if (exactCasing || _headerNameMode == HeaderNameMode.Canonical)
				{
					return canonical;
				}
			}

			return MaterialiseHeaderName(name);
		}

		/// <summary>
		/// Applies the framing headers the parser acts on. This replaces the four case-insensitive
		/// literals that used to sit in the Ragel grammar as an alternation beside http_token.
		/// </summary>
		private void ApplyKnownHeader(KnownHeader header)
		{
			switch (header)
			{
				case KnownHeader.ContentLength:
					if (_contentLength != -1) throw new Exception("Already got Content-Length. Possible attack?");
					_contentLength = 0;
					inContentLengthHeader = true;
					break;

				case KnownHeader.Connection:
					inConnectionHeader = true;
					break;

				case KnownHeader.TransferEncoding:
					inTransferEncodingHeader = true;
					break;

				case KnownHeader.Upgrade:
					inUpgradeHeader = true;
					break;
			}
		}

		/// <summary>
		/// Widens the name's bytes to chars one for one, reproducing exactly what the old
		/// StringBuilder path did with <c>(char)fc</c>. Deliberately not an Encoding call: this
		/// behaves identically on every target framework with no encoding dependency, and header
		/// names are restricted to ASCII token bytes by the grammar anyway.
		/// </summary>
		private string MaterialiseHeaderName(ReadOnlySpan<byte> name)
		{
			for (var i = 0; i < name.Length; i++)
			{
				_headerNameChars[i] = (char)name[i];
			}

			return new string(_headerNameChars, 0, name.Length);
		}

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
				
#line 635 "../HttpCombinedParser.cs"
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
	case 39:
#line 454 "HttpParser2-chunked.cs.rl"
	{
			throw new Exception("Parser is dead; there shouldn't be more data. Client is bogus? fpc =" + p);
		}
	break;
#line 656 "../HttpCombinedParser.cs"
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
#line 75 "HttpParser2-chunked.cs.rl"
	{
			_stringBuilder.Append((char)data[p]);
		}
	break;
	case 1:
#line 79 "HttpParser2-chunked.cs.rl"
	{
			_stringBuilder.Length = 0;
		}
	break;
	case 2:
#line 83 "HttpParser2-chunked.cs.rl"
	{
			_stringBuilder2.Append((char)data[p]);
		}
	break;
	case 3:
#line 87 "HttpParser2-chunked.cs.rl"
	{
			if (_stringBuilder2 == null)
				_stringBuilder2 = new StringBuilder();
		 	_stringBuilder2.Length = 0;
		}
	break;
	case 4:
#line 93 "HttpParser2-chunked.cs.rl"
	{
			_chunkLength = (_chunkLength << 4) | HexValue((char)data[p]);
		}
	break;
	case 5:
#line 97 "HttpParser2-chunked.cs.rl"
	{
			_chunkLength = 0;
		}
	break;
	case 6:
#line 101 "HttpParser2-chunked.cs.rl"
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
#line 118 "HttpParser2-chunked.cs.rl"
	{
           //Console.WriteLine("matched absolute_uri");
        }
	break;
	case 8:
#line 121 "HttpParser2-chunked.cs.rl"
	{
            //Console.WriteLine("matched abs_path");
        }
	break;
	case 9:
#line 124 "HttpParser2-chunked.cs.rl"
	{
            //Console.WriteLine("matched authority");
        }
	break;
	case 10:
#line 127 "HttpParser2-chunked.cs.rl"
	{
            //Console.WriteLine("matched first space");
        }
	break;
	case 11:
#line 130 "HttpParser2-chunked.cs.rl"
	{
            //Console.WriteLine("leave_first_space");
        }
	break;
	case 13:
#line 139 "HttpParser2-chunked.cs.rl"
	{
			//Console.WriteLine("matched_leading_crlf");
		}
	break;
	case 14:
#line 149 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnMethod(this, _stringBuilder.ToString());
		}
	break;
	case 15:
#line 153 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnRequestUri(this, _stringBuilder.ToString());
		}
	break;
	case 16:
#line 158 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnPath(this, _stringBuilder2.ToString());
		}
	break;
	case 17:
#line 163 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnQueryString(this, _stringBuilder2.ToString());
		}
	break;
	case 18:
#line 168 "HttpParser2-chunked.cs.rl"
	{
			statusCode = 0;
		}
	break;
	case 19:
#line 173 "HttpParser2-chunked.cs.rl"
	{
			statusCode = statusCode * 10 + ((char)data[p] - '0');
		}
	break;
	case 20:
#line 178 "HttpParser2-chunked.cs.rl"
	{
			statusReason = _stringBuilder.ToString();
		}
	break;
	case 21:
#line 183 "HttpParser2-chunked.cs.rl"
	{
			_isRequest = true;
			parserDelegate.OnRequestType(this);
		}
	break;
	case 22:
#line 189 "HttpParser2-chunked.cs.rl"
	{
			_isRequest = false;
			parserDelegate.OnResponseType(this);
			parserDelegate.OnResponseCode(this, statusCode, statusReason);
			statusReason = null;
			statusCode = 0;
		}
	break;
	case 23:
#line 207 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnFragment(this, _stringBuilder2.ToString());
		}
	break;
	case 24:
#line 220 "HttpParser2-chunked.cs.rl"
	{
			MajorVersion = (char)data[p] - '0';
		}
	break;
	case 25:
#line 224 "HttpParser2-chunked.cs.rl"
	{
			MinorVersion = (char)data[p] - '0';
		}
	break;
	case 26:
#line 228 "HttpParser2-chunked.cs.rl"
	{
			//Console.WriteLine("header_connection_close");
			if (inConnectionHeader)
				gotConnectionClose = true;
		}
	break;
	case 27:
#line 234 "HttpParser2-chunked.cs.rl"
	{
			//Console.WriteLine("header_connection_keepalive");
			if (inConnectionHeader)
				gotConnectionKeepAlive = true;
		}
	break;
	case 28:
#line 240 "HttpParser2-chunked.cs.rl"
	{
			if (inTransferEncodingHeader)
				gotTransferEncodingChunked = true;
            parserDelegate.OnTransferEncodingChunked(this, true);
			Debug.WriteLine($"Transfer Encoding Chunked: {gotTransferEncodingChunked}");
		}
	break;
	case 29:
#line 251 "HttpParser2-chunked.cs.rl"
	{
			_headerNameLength = 0;
		}
	break;
	case 30:
#line 255 "HttpParser2-chunked.cs.rl"
	{
			if (_headerNameLength == _headerNameBytes.Length)
			{
				GrowHeaderNameBuffer();
			}

			_headerNameBytes[_headerNameLength++] = data[p];
		}
	break;
	case 31:
#line 264 "HttpParser2-chunked.cs.rl"
	{
			parserDelegate.OnHeaderName(this, ResolveHeaderName());
		}
	break;
	case 32:
#line 268 "HttpParser2-chunked.cs.rl"
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
	case 33:
#line 280 "HttpParser2-chunked.cs.rl"
	{
			Debug.WriteLine($"Chunk Length: {_chunkLength}");
			parserDelegate.OnChunkedLength(this, _chunkLength);
        }
	break;
	case 34:
#line 285 "HttpParser2-chunked.cs.rl"
	{
			
			if (data[p] == 10)
			{
				//Console.WriteLine("leave_headers contentLength = " + contentLength);
				parserDelegate.OnHeadersEnd(this);

				// if chunked transfer, ignore content length and parse chunked
				// if content length given but zero, read next request
				// if content length is given and non-zero, we should read that many bytes
				// if content length is not given
				//   if this is a request, the body length is zero (RFC 9112 6.3 rule 6)
				//   else (response, unframed) it depends on UnframedResponseMode:
				//     CompleteAtHeaders + keep-alive -> complete now (HTTPU/SSDP)
				//     otherwise                       -> close-delimited, read until EOF

				if (gotTransferEncodingChunked)
				{
					// RFC 9112 6.3: Transfer-Encoding takes precedence over Content-Length
					{cs = 99;if (true) goto _again;}
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
					{cs = 98;if (true) goto _again;}
				}
				else if (_isRequest)
				{
					// RFC 9112 6.3 rule 6: a request with neither Transfer-Encoding nor
					// Content-Length has a body length of zero. Close-delimited bodies
					// are a response-only rule.
					parserDelegate.OnMessageEnd(this);
					{cs = 1;if (true) goto _again;}
				}
				else if (_unframedResponseMode == UnframedResponseMode.CompleteAtHeaders && ShouldKeepAlive)
				{
					// A bodyless keep-alive response is treated as complete at the end of its
					// headers (HTTPU/SSDP semantics). See UnframedResponseMode / the README.
					parserDelegate.OnMessageEnd(this);
					{cs = 1;if (true) goto _again;}
				}
				else
				{
					// No framing information: the body runs until the connection closes
					// (RFC 9112 6.3). Signal EOF by calling Execute with an empty buffer.
					// Reached for Connection: close, or for any unframed response when
					// UnframedResponseMode.CloseDelimited was selected.
					{cs = 107;if (true) goto _again;}
				}
			}
        }
	break;
	case 35:
#line 343 "HttpParser2-chunked.cs.rl"
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
						{cs = 104;if (true) goto _again;}
					}
				}
				else
				{
					{p++; if (true) goto _out; }
				}
			}
		}
	break;
	case 36:
#line 376 "HttpParser2-chunked.cs.rl"
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
					{cs = 99;if (true) goto _again;}
				}
				else
				{
					// Additional chunk data will be present in the next buffer. Stay in the same sate.
					{cs = 108;if (true) goto _again;}
				}
			}

			if (_chunkLength == 0)
			{
				{cs = 110;if (true) goto _again;}
			}
			else
			{
				{p++; if (true) goto _out; }
			}
		}
	break;
	case 37:
#line 409 "HttpParser2-chunked.cs.rl"
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
				{cs = 104;if (true) goto _again;}
			}
		}
	break;
	case 38:
#line 425 "HttpParser2-chunked.cs.rl"
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
					{cs = 104;if (true) goto _again;}
				}
			}
		}
	break;
#line 1099 "../HttpCombinedParser.cs"
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
#line 133 "HttpParser2-chunked.cs.rl"
	{
            //Console.WriteLine("eof_leave_first_space");
        }
	break;
	case 36:
#line 376 "HttpParser2-chunked.cs.rl"
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
					{cs = 99;if (true) goto _again;}
				}
				else
				{
					// Additional chunk data will be present in the next buffer. Stay in the same sate.
					{cs = 108;if (true) goto _again;}
				}
			}

			if (_chunkLength == 0)
			{
				{cs = 110;if (true) goto _again;}
			}
			else
			{
				{p++; if (true) goto _out; }
			}
		}
	break;
	case 38:
#line 425 "HttpParser2-chunked.cs.rl"
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
					{cs = 104;if (true) goto _again;}
				}
			}
		}
	break;
#line 1182 "../HttpCombinedParser.cs"
		default: break;
		}
	}
	}

	_out: {}
	}

#line 674 "HttpParser2-chunked.cs.rl"
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