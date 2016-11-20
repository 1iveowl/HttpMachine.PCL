.\build.ps1

$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\main\bin\release\HttpMachine.dll')).Version.ToString(3)

nuget.exe push -Source "1iveowlNuGetRepo" -ApiKey key .\NuGet\HttpMachine.PCL.$version.symbols.nupkg