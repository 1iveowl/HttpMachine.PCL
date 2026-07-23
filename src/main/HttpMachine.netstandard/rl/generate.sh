#!/usr/bin/env bash
# Regenerates ../HttpCombinedParser.cs from the Ragel grammar.
# Requires ragel 6.x on PATH (e.g. `apt install ragel`).
#
# Ragel 6.x's C# backend declares every table as sbyte[]/short[] based on
# element count, not value range; several tables hold values above 127 and
# would not compile as sbyte. The sed step widens those tables to byte[].
set -euo pipefail
cd "$(dirname "$0")"

ragel -A -T0 -o ../HttpCombinedParser.cs HttpParser2-chunked.cs.rl

sed -i -E \
    '/_http_parser_(indicies|trans_targs|trans_actions|from_state_actions|eof_actions)/ s/sbyte/byte/g' \
    ../HttpCombinedParser.cs

echo "Generated ../HttpCombinedParser.cs"
