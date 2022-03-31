param([string]$betaver)

if ([string]::IsNullOrEmpty($betaver)) {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\main\HttpMachine.netstandard\bin\Release\netstandard2.0\HttpMachine.dll')).Version.ToString(3)
	}
else {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\main\HttpMachine.netstandard\bin\Release\netstandard2.0\HttpMachine.dll')).Version.ToString(3) + "-" + $betaver
}

.\build.ps1 $version

c:\tools\nuget\Nuget.exe push ".\NuGet\HttpMachine.PCL.$version.symbols.nupkg" -Source https://www.nuget.org