#!/usr/bin/env bash
# Runs the end-to-end parser benchmark against both the working tree and an earlier revision, in a
# single process, so the comparison gets a real ratio column instead of relying on two separate runs
# being comparable. They are not: repeated runs of an identical binary in a container drift by
# several percent, which is the same magnitude as the differences being measured.
#
# The obstacle is assembly identity. Referencing the released HttpMachine.dll alongside the locally
# built one fails however the "extern alias" is arranged, because both are HttpMachine 6.1.0.0 and
# MSBuild's conflict resolution drops one before the compiler sees the alias. The fix is to give the
# baseline a different identity: build the old parser from a worktree with AssemblyName changed, and
# then the alias works as intended.
#
# Usage: ./benchmark-against-head.sh [git-ref]     (default: HEAD)
set -euo pipefail
cd "$(dirname "$0")"

ref="${1:-HEAD}"
repo_root="$(git rev-parse --show-toplevel)"
scratch="$(mktemp -d)"
worktree="${scratch}/baseline"
project="${scratch}/comparison"
benchmarks="${repo_root}/src/benchmarks/HeaderMatcher.Benchmarks"

cleanup() {
    git -C "${repo_root}" worktree remove --force "${worktree}" 2>/dev/null || true
    rm -rf "${scratch}"
}
trap cleanup EXIT

echo "Building the parser at ${ref} as HttpMachineBaseline"
git -C "${repo_root}" worktree add --detach "${worktree}" "${ref}" >/dev/null

baseline_project="${worktree}/src/main/HttpMachine.netstandard"

# Renaming via -p:AssemblyName= would apply to every project in the graph, including IHttpMachine,
# so the property is changed in the throwaway worktree's project file instead.
sed -i 's|<AssemblyName>HttpMachine</AssemblyName>|<AssemblyName>HttpMachineBaseline</AssemblyName>|' \
    "${baseline_project}/HttpMachine.netstandard.csproj"

dotnet build "${baseline_project}/HttpMachine.netstandard.csproj" -c Release -f net10.0 --nologo -v quiet

baseline_dll="${baseline_project}/bin/Release/net10.0/HttpMachineBaseline.dll"

if [ ! -f "${baseline_dll}" ]; then
    echo "Baseline assembly was not produced at ${baseline_dll}" >&2
    exit 1
fi

mkdir -p "${project}"

cat > "${project}/Comparison.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>14.0</LangVersion>
    <Nullable>enable</Nullable>
    <AssemblyName>Comparison</AssemblyName>
    <RootNamespace>HeaderMatcher.Benchmarks</RootNamespace>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="BenchmarkDotNet" Version="0.15.4" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="${repo_root}/src/main/HttpMachine.netstandard/HttpMachine.netstandard.csproj" />
    <ProjectReference Include="${repo_root}/src/interface/IHttpMachine/IHttpMachine.csproj" />
    <!-- Distinct assembly identity, so the alias survives conflict resolution. -->
    <Reference Include="${baseline_dll}" Aliases="baseline" />
  </ItemGroup>
  <ItemGroup>
    <!-- The identical corpus and delegate, so both parsers see exactly the same input. -->
    <Compile Include="${benchmarks}/Messages.cs" />
    <Compile Include="${benchmarks}/NullDelegate.cs" />
    <Compile Include="Program.cs" />
  </ItemGroup>
</Project>
EOF

cat > "${project}/Program.cs" <<'EOF'
extern alias baseline;

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using HttpMachine;
using BaselineParser = baseline::HttpMachine.HttpCombinedParser;

namespace HeaderMatcher.Benchmarks;

[MemoryDiagnoser]
public class Comparison
{
    private byte[] _message = null!;

    private BaselineParser _before = null!;
    private HttpCombinedParser _asReceived = null!;
    private HttpCombinedParser _canonical = null!;

    private BaselineParser _beforeReal = null!;
    private HttpCombinedParser _asReceivedReal = null!;
    private HttpCombinedParser _canonicalReal = null!;

    [Params("Request", "Response", "SsdpNotify")]
    public string Message { get; set; } = "Request";

    [Params(false, true)]
    public bool RealDelegate { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _message = Messages.Get(Message);

        _before = new BaselineParser(new NullDelegate());
        _asReceived = new HttpCombinedParser(new NullDelegate(), HeaderNameMode.AsReceived);
        _canonical = new HttpCombinedParser(new NullDelegate(), HeaderNameMode.Canonical);

        _beforeReal = new BaselineParser(new HttpParserDelegate());
        _asReceivedReal = new HttpCombinedParser(new HttpParserDelegate(), HeaderNameMode.AsReceived);
        _canonicalReal = new HttpCombinedParser(new HttpParserDelegate(), HeaderNameMode.Canonical);

        // Comparing parsers that disagree would be meaningless, and a parser that quietly bails out
        // just measures how fast it gives up. Check all three consume everything before timing.
        Check(_before.Execute(_message), "before");
        Check(_asReceived.Execute(_message), "AsReceived");
        Check(_canonical.Execute(_message), "Canonical");
    }

    private void Check(int consumed, string which)
    {
        if (consumed != _message.Length)
        {
            throw new InvalidOperationException(
                $"{which} consumed {consumed} of {_message.Length} bytes of the {Message} message.");
        }
    }

    [Benchmark(Baseline = true, Description = "before")]
    public int Before() => RealDelegate ? _beforeReal.Execute(_message) : _before.Execute(_message);

    [Benchmark(Description = "AsReceived")]
    public int AsReceived() => RealDelegate ? _asReceivedReal.Execute(_message) : _asReceived.Execute(_message);

    [Benchmark(Description = "Canonical")]
    public int Canonical() => RealDelegate ? _canonicalReal.Execute(_message) : _canonical.Execute(_message);
}

internal static class Program
{
    private static void Main(string[] args) =>
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
EOF

echo "Running the comparison"

# BenchmarkDotNet rebuilds a boilerplate project at run time and resolves the .csproj relative to
# the working directory, not to --project.
cd "${project}"
dotnet run -c Release -- --filter '*'
