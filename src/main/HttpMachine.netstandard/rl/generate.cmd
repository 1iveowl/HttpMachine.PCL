@echo off
rem Regenerates ..\HttpCombinedParser.cs from the Ragel grammar.
rem Requires ragel 6.x on PATH. After generating, widen the sbyte[] tables
rem that hold values above 127 to byte[] (see generate.sh), or run this
rem repo's generate.sh from WSL/git-bash which does it automatically.
ragel -A -T0 -o ..\HttpCombinedParser.cs HttpParser2-chunked.cs.rl
