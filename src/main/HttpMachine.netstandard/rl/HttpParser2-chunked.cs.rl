using System;
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

    private int cs;
    // int mark;
    private int statusCode;
    private string statusReason;

		/// <summary>Releases resources. The parser holds none.</summary>
		public void Dispose()
		{

		}

    %%{

        # define actions
        machine http_parser;

		action buf {
			_stringBuilder.Append((char)fc);
		}

		action clear {
			_stringBuilder.Length = 0;
		}

		action buf2 {
			_stringBuilder2.Append((char)fc);
		}

		action clear2 {
			if (_stringBuilder2 == null)
				_stringBuilder2 = new StringBuilder();
		 	_stringBuilder2.Length = 0;
		}

		action chunked_hex_buf {
			_chunkLength = (_chunkLength << 4) | HexValue((char)fc);
		}

		action chunked_hex_clear {
			_chunkLength = 0;
		}

		action message_begin {
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
			parserDelegate.OnMessageBegin(this);
		}
        
        action matched_absolute_uri {
           //Console.WriteLine("matched absolute_uri");
        }
        action matched_abs_path {
            //Console.WriteLine("matched abs_path");
        }
        action matched_authority {
            //Console.WriteLine("matched authority");
        }
        action matched_first_space {
            //Console.WriteLine("matched first space");
        }
        action leave_first_space {
            //Console.WriteLine("leave_first_space");
        }
        action eof_leave_first_space {
            //Console.WriteLine("eof_leave_first_space");
        }
		action matched_header { 
			//Console.WriteLine("matched header");
		}
		action matched_leading_crlf {
			//Console.WriteLine("matched_leading_crlf");
		}
		action matched_last_crlf_before_body {
			//Console.WriteLine("matched_last_crlf_before_body");
		}
		action matched_header_crlf {
			//Console.WriteLine("matched_header_crlf");
		}

		action on_method {
			parserDelegate.OnMethod(this, _stringBuilder.ToString());
		}
        
		action on_request_uri {
			parserDelegate.OnRequestUri(this, _stringBuilder.ToString());
		}

		action on_abs_path
		{
			parserDelegate.OnPath(this, _stringBuilder2.ToString());
		}
        
		action on_query_string
		{
			parserDelegate.OnQueryString(this, _stringBuilder2.ToString());
		}

		action status_code_clear
		{
			statusCode = 0;
		}

		action status_code_digit
		{
			statusCode = statusCode * 10 + ((char)fc - '0');
		}

		action status_reason
		{
			statusReason = _stringBuilder.ToString();
		}
		
		action on_request_message
		{
			parserDelegate.OnRequestType(this);
		}

		action on_response_message
		{
			parserDelegate.OnResponseType(this);
			parserDelegate.OnResponseCode(this, statusCode, statusReason);
			statusReason = null;
			statusCode = 0;
		}

        action enter_query_string {
            //Console.WriteLine("enter_query_string fpc " + fpc);
            qsMark = fpc;
        }

        action leave_query_string {
            parserDelegate.OnQueryString(this, new ArraySegment<byte>(data, qsMark, fpc - qsMark));
        }

		action on_fragment
		{
			parserDelegate.OnFragment(this, _stringBuilder2.ToString());
		}

        action enter_fragment {
            //Console.WriteLine("enter_fragment fpc " + fpc);
            fragMark = fpc;
        }

        action leave_fragment {
			parserDelegate.OnFragment(this, new ArraySegment<byte>(data, fragMark, fpc - fragMark));
        }

        action version_major {
			MajorVersion = (char)fc - '0';
		}

		action version_minor {
			MinorVersion = (char)fc - '0';
		}
		
        action header_content_length {
            if (_contentLength != -1) throw new Exception("Already got Content-Length. Possible attack?");
			//Console.WriteLine("Saw content length");
			_contentLength = 0;
			inContentLengthHeader = true;
        }

		action header_connection {
			//Console.WriteLine("header_connection");
			inConnectionHeader = true;
		}

		action header_connection_close {
			//Console.WriteLine("header_connection_close");
			if (inConnectionHeader)
				gotConnectionClose = true;
		}

		action header_connection_keepalive {
			//Console.WriteLine("header_connection_keepalive");
			if (inConnectionHeader)
				gotConnectionKeepAlive = true;
		}
		
		action header_transfer_encoding {
			//Console.WriteLine("Saw transfer encoding");
			inTransferEncodingHeader = true;
		}

		action header_transfer_encoding_chunked {
			if (inTransferEncodingHeader)
				gotTransferEncodingChunked = true;
            parserDelegate.OnTransferEncodingChunked(this, true);
			Debug.WriteLine($"Transfer Encoding Chunked: {gotTransferEncodingChunked}");
		}

		action header_upgrade {
			inUpgradeHeader = true;
		}

		action on_header_name {
			parserDelegate.OnHeaderName(this, _stringBuilder.ToString());
		}

		action on_header_value {
			var str = _stringBuilder.ToString();
			//Console.WriteLine("on_header_value '" + str + "'");
			//Console.WriteLine("inContentLengthHeader " + inContentLengthHeader);
			if (inContentLengthHeader)
				_contentLength = int.Parse(str);

			inConnectionHeader = inTransferEncodingHeader = inContentLengthHeader = false;
			
			parserDelegate.OnHeaderValue(this, str);
		}

        action on_chunck_len_hex {
			Debug.WriteLine($"Chunk Length: {_chunkLength}");
			parserDelegate.OnChunkedLength(this, _chunkLength);
        }

        action last_crlf {
			
			if (fc == 10)
			{
				//Console.WriteLine("leave_headers contentLength = " + contentLength);
				parserDelegate.OnHeadersEnd(this);

				// if chunked transfer, ignore content length and parse chunked
				// if content length given but zero, read next request
				// if content length is given and non-zero, we should read that many bytes
				// if content length is not given
				//   if should keep alive, assume next request is coming and read it
				//   else read the body until the connection closes (EOF)

				if (gotTransferEncodingChunked)
				{
					// RFC 9112 6.3: Transfer-Encoding takes precedence over Content-Length
					fgoto body_chunked_identity;
				}
				else if (_contentLength == 0)
				{
					// No Content. Get ready for new incoming request
					parserDelegate.OnMessageEnd(this);
					fgoto main;
				}
				else if (_contentLength > 0)
				{
					// Handle Body based on Content Length
					fgoto body_identity;
				}
				else
				{
					if (ShouldKeepAlive)
					{
						parserDelegate.OnMessageEnd(this);
						fgoto main;
					}
					else
					{
						// No framing information: the body runs until the connection closes.
						// Signal EOF by calling Execute with an empty buffer.
						fgoto body_identity_eof;
					}
				}
			}
        }

		action body_identity {
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
						fgoto main;
					}
					else
					{
						//fhold;
						fgoto dead;
					}
				}
				else
				{
					fbreak;
				}
			}
		}

		action read_chunk {
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
					fgoto body_chunked_identity;
				}
				else
				{
					// Additional chunk data will be present in the next buffer. Stay in the same sate.
					fgoto chunk;
				}
			}

			if (_chunkLength == 0)
			{
				fgoto read_chunk_stop;
			}
			else
			{
				fbreak;
			}
		}

		action read_chunk_stop {
			//Debug.WriteLine($"End of chunks");

			parserDelegate.OnMessageEnd(this);
			
			if (ShouldKeepAlive)
			{
				fgoto main;
			}
			else
			{
				fhold;
				fgoto dead;
			}
		}
		
		action body_identity_eof {
			var toRead = pe - p;
			Debug.WriteLine($"Eof To Read: {toRead}");
			if (toRead > 0)
			{
				EmitBody(data, array, arrayOffset, p, toRead);
				p += toRead - 1;
				fbreak;
			}
			else
			{
				// Reached only from the EOF action (empty Execute call).
				parserDelegate.OnMessageEnd(this);

				if (ShouldKeepAlive)
				{
					fhold;
					fgoto main;
				}
				else
				{
					fhold;
					fgoto dead;
				}
			}
		}



		action enter_dead {
			throw new Exception("Parser is dead; there shouldn't be more data. Client is bogus? fpc =" + fpc);
		}

        include http "http-chunked.rl";
        
    }%%

    %% write data;

    /// <summary>Initializes the state machine. Use the delegate-taking constructor instead.</summary>
    protected HttpCombinedParser()
    {
			_stringBuilder = new StringBuilder();
        %% write init;
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
				%% write exec;
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