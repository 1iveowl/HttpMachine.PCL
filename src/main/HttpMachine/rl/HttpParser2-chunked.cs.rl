using System;
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

		action chunked_body_clear {
			_chunkedBufferBuilder.Clear();
		}

		action chunked_hex_buf {
			_chunkedHexBufferBuilder.Append((char)fc);
		}

		action chunked_hex_clear {
			_chunkedHexBufferBuilder.Clear();
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
			gotUpgradeValue = false;
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

		action status_code
		{
			statusCode = int.Parse(_stringBuilder.ToString());
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
            _chunkLength = Convert.ToInt32(_chunkedHexBufferBuilder.ToString(), 16);
			_chunkPos = _chunkLength;
			Debug.WriteLine($"Chunk Length: {_chunkLength}");	
			parserDelegate.OnChunkedLength(this, _chunkLength);	
			
        }

        action last_crlf {
			
			if (fc == 10)
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
					fgoto main;
				}
				else if (_contentLength > 0)
				{
					// Handle Body based on Content Length
					fgoto body_identity;
				}
				else if (gotTransferEncodingChunked)
				{
					// Handle Body based on Transfer-Encoding Chunked Length
					fgoto body_chunked_identity;
				}
				else
				{
					if (ShouldKeepAlive)
					{
						parserDelegate.OnMessageEnd(this);
						fgoto main;
					}
				}
			}
        }

		action body_identity {
			var toRead = Math.Min(pe - p, _contentLength);
			//Console.WriteLine("body_identity: reading " + toRead + " bytes from body.");
			if (toRead > 0)
			{
				parserDelegate.OnBody(this, new ArraySegment<byte>(data, p, toRead), toRead);
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
			Debug.WriteLine($"Reading chunk size: {_chunkLength}.");// p={p}, pe={pe}");
			var toRead = Math.Min(pe - p, _chunkLength);
			if (toRead > 0)
			{
				Debug.WriteLine($"To Read: {toRead}");
				parserDelegate.OnChunkReceived(this);
				parserDelegate.OnBody(this, new ArraySegment<byte>(data, p, toRead), toRead, isChunked:true);
				p += toRead - 1;
				_chunkLength -= toRead;
				
				fgoto body_chunked_identity;
			}

			if (_chunkLength == 0)
			{
				Debug.WriteLine($"EoF Chunk identified");
				parserDelegate.OnMessageEnd(this);
				fgoto body_identity_eof;
			}
			else
			{
				fbreak;
			}
		}
		
		action body_identity_eof {
			var toRead = pe - p;
			Debug.WriteLine($"Eof To Read: {toRead}");
			//Console.WriteLine("body_identity_eof: reading " + toRead + " bytes from body.");
			if (toRead > 0)
			{
				if (gotTransferEncodingChunked)
				{
					parserDelegate.OnBody(this, new ArraySegment<byte>(data, p, toRead), toRead, isChunked:true);
					p += toRead - 1;
					fbreak;
				}
				else
				{
					parserDelegate.OnBody(this, new ArraySegment<byte>(data, p, toRead), toRead);
					p += toRead - 1;
					fbreak;
				}
				parserDelegate.OnBody(this, new ArraySegment<byte>(data, p, toRead), toRead);
				p += toRead - 1;
				fbreak;
			}
			else
			{
				parserDelegate.OnMessageEnd(this);
				
				if (ShouldKeepAlive)
					fgoto main;
				else
				{
					//Console.WriteLine("body_identity_eof: going to dead");
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
        
        protected HttpCombinedParser()
        {
			_stringBuilder = new StringBuilder();
			_chunkedBufferBuilder = new StringBuilder();
			_chunkedHexBufferBuilder = new StringBuilder();
            %% write init;        
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
				%% write exec;
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