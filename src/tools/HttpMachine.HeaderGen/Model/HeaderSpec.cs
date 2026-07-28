namespace HttpMachine.HeaderGen.Model;

/// <summary>One entry from KnownHeaders.txt, after parsing and validation.</summary>
/// <param name="Name">The canonical header name, exactly as written in the input file.</param>
/// <param name="EnumMember">The <c>KnownHeader</c> member name for this header.</param>
/// <remarks>
/// Note what is <em>not</em> here: the line number it came from. Diagnostics carry their own
/// <see cref="LocationInfo"/>, so keeping the line out of the model means that editing a comment
/// in KnownHeaders.txt shifts no values, the model compares equal, and the emitter does not
/// re-run. Putting a line number on every header would make every comment edit look like a
/// content change.
/// </remarks>
internal readonly record struct HeaderSpec(string Name, string EnumMember);
