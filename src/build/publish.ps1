.\build.ps1

$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\main\bin\release\HttpMachine.dll')).Version.ToString(3)

Nuget.exe push ".\NuGet\HttpMachine.PCL.$version.symbols.nupkg" -Source https://www.nuget.org