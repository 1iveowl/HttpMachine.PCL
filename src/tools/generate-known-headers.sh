#!/usr/bin/env bash
# Regenerates ../main/HttpMachine.netstandard/Generated/KnownHeaders.g.cs from
# ../main/HttpMachine.netstandard/headers/KnownHeaders.txt.
#
# Requires only the .NET SDK - the generator is an ordinary incremental source generator that
# runs as an analyzer in HttpMachine.HeaderGen.Emit's build. Nothing here is referenced by the
# shipping library or its NuGet packages; the committed output is, exactly like the
# Ragel-generated parser (see rl/generate.sh).
#
# The emit project builds against every framework HttpMachine targets, so this also proves the
# generated matcher compiles down to netstandard2.0. The script then asserts that all five
# frameworks produced a byte-identical file, which is what guarantees there is one matcher
# implementation rather than one per framework.
set -euo pipefail
cd "$(dirname "$0")"

emit_project="HttpMachine.HeaderGen.Emit"
destination="../main/HttpMachine.netstandard/Generated/KnownHeaders.g.cs"

# Stale output from a previous run would otherwise survive a build that generated nothing.
rm -rf "${emit_project}/obj/emitted"

# --no-incremental is required, not tidiness: deleting the emitted files above does not make the
# project out of date, so an incremental build skips the compiler entirely and emits nothing.
dotnet build "${emit_project}/${emit_project}.csproj" --nologo -v quiet --no-incremental

mapfile -t generated < <(find "${emit_project}/obj/emitted" -name 'KnownHeaders.g.cs' | sort)

if [ "${#generated[@]}" -eq 0 ]; then
    echo "No KnownHeaders.g.cs was generated. Did the analyzer fail to load?" >&2
    exit 1
fi

distinct=$(md5sum "${generated[@]}" | awk '{print $1}' | sort -u | wc -l)

if [ "${distinct}" -ne 1 ]; then
    echo "The target frameworks produced different output; the matcher must be identical for all of them:" >&2
    md5sum "${generated[@]}" >&2
    exit 1
fi

mkdir -p "$(dirname "${destination}")"
cp "${generated[0]}" "${destination}"

echo "Generated ${destination} from ${#generated[@]} identical target-framework builds"
