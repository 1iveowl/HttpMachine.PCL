param([string]$version)

if ([string]::IsNullOrEmpty($version)) {$version = "0.0.1"}

$msbuild = join-path -path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" -childpath "msbuild.exe"
&$msbuild ..\main\HttpMachine.netstandard\HttpMachine.netstandard.csproj /t:Build /p:Configuration="Release"



Remove-Item .\NuGet -Force -Recurse
New-Item -ItemType Directory -Force -Path .\NuGet

NuGet.exe pack HttpMachine.nuspec -Verbosity detailed -Symbols -OutputDir "NuGet" -Version $version